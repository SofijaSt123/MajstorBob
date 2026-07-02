const tipKorisnika = localStorage.getItem('tip_korisnika');
if (tipKorisnika?.toLowerCase() !== 'klijent') {
    window.location.href = 'index.html';
}

let sveRezervacije = [];
let filtriraneRezervacije = [];
let prikazanaPonudaId = null;

async function dohvatiImeIzvodjaca(izvodjacId) {
    let majstor = null;
    try {
        majstor = await getMajstorInfoForKlijent(izvodjacId);
    } catch (e) {
        console.log(`Majstor sa ID ${izvodjacId} nije pronađen, pokušavam kao firma...`);
    }
    
    if (majstor && majstor.ime) {
        return `${majstor.ime} ${majstor.prezime || ''}`.trim();
    }
    
    let firma = null;
    try {
        firma = await getFirmaInfoForKlijent(izvodjacId);
    } catch (e2) {
        console.warn(`Ne mogu da dohvatim podatke za korisnika ${izvodjacId}`);
    }
    
    if (firma && firma.naziv_firme) {
        return firma.naziv_firme; 
    }
    if (firma && firma.ime) {
        return `${firma.ime} ${firma.prezime || ''}`.trim();
    }
    
    return `Izvođač #${izvodjacId}`;
}

async function ucitajRezervacije() {
    const tbody = document.getElementById('tabelaRezervacija');
    tbody.innerHTML = '<tr><td colspan="6" class="text-center">Učitavanje rezervacija...</td></tr>';
    
    try {
        const zahtevi = await getZahtevi();
        const zakazivanja = await getZakazivanja();
        
        let sveRezervacijeTemp = [];
        
        const zahteviSaZakazivanjem = new Set();
        for (const z of zakazivanja) {
            zahteviSaZakazivanjem.add(z.id_zahteva);
        }
        
        for (const z of zahtevi) {
            if (zahteviSaZakazivanjem.has(z.id_zahteva)) {
                continue;  
            }
            
            const imeMajstora = await dohvatiImeIzvodjaca(z.id_izvodjaca);
            
            let status = 'pending';
            if (z.status === 1) status = 'confirmed';
            if (z.status === 2) status = 'rejected';
            
            sveRezervacijeTemp.push({
                id: z.id_zahteva,
                id_zakazivanja: null,
                id_zahteva: z.id_zahteva,
                majstor: imeMajstora,
                datum: z.datum,
                vreme: z.vreme,
                opis: z.opis_radova,
                status: status,
                cena: 0,
                placeno: false,
                cenaDonja: null,
                cenaGornja: null,
                konacnaCena: null,
                pocetak: null,
                kraj: null,
                jeZahtev: true
            });
        }
        
        for (const z of zakazivanja) {
            let imeMajstora = 'Nepoznati majstor';
            
            const zahtev = zahtevi.find(req => req.id_zahteva === z.id_zahteva);
            if (zahtev && zahtev.id_izvodjaca) {
                imeMajstora = await dohvatiImeIzvodjaca(zahtev.id_izvodjaca);
            }
            
            sveRezervacijeTemp.push({
                id: z.id_zakazivanja,
                id_zakazivanja: z.id_zakazivanja,
                id_zahteva: z.id_zahteva,
                majstor: imeMajstora,
                datum: z.datum,
                vreme: z.pocetak || '',
                opis: z.opis || '',
                status: mapirajStatus(z.status),
                cena: z.cena_gornja || z.cena_donja || 0,
                placeno: z.da_li_je_placeno || false,
                cenaDonja: z.cena_donja,
                cenaGornja: z.cena_gornja,
                konacnaCena: z.konacna_cena || null,
                pocetak: z.pocetak,
                kraj: z.kraj,
                jeZahtev: false
            });
        }
        
        sveRezervacijeTemp.sort((a, b) => new Date(b.datum) - new Date(a.datum));
        
        sveRezervacije = sveRezervacijeTemp;
        filtriraneRezervacije = [...sveRezervacije];
        prikaziRezervacije();
        
    } catch (error) {
        console.error('Greška:', error);
        tbody.innerHTML = '<tr><td colspan="6" class="text-center tekst-crven">Greška pri učitavanju rezervacija</td></tr>';
    }
}

function mapirajStatus(status) {
    switch(status) {
        case 0: return 'pending';
        case 1: return 'rejected';
        case 2: return 'confirmed';
        case 3: return 'cancelled';
        default: return 'pending';
    }
}

function mapirajStatusTekst(status) {
    switch(status) {
        case 'pending': return '<i class="fa-solid fa-clock"></i> Na čekanju';
        case 'confirmed': return '<i class="fa-solid fa-circle-check"></i> Potvrđeno';
        case 'rejected': return '<i class="fa-solid fa-circle-xmark"></i> Odbijeno';
        case 'cancelled': return '<i class="fa-solid fa-ban"></i> Otkazano';
        default: return '<i class="fa-solid fa-question"></i> Nepoznat';
    }
}

function mapirajStatusKlasu(status) {
    switch(status) {
        case 'pending': return 'status-cekanje';
        case 'confirmed': return 'status-potvrdjeno';
        case 'rejected': return 'status-odbijeno';
        case 'cancelled': return 'status-odbijeno';
        default: return '';
    }
}

function prikaziRezervacije() {
    const tbody = document.getElementById('tabelaRezervacija');
    
    if (!filtriraneRezervacije || filtriraneRezervacije.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">Nema rezervacija za prikaz</td></tr>';
        return;
    }
    
    let html = '';
    for (let r of filtriraneRezervacije) {
        const statusKlasa = mapirajStatusKlasu(r.status);
        const statusTekst = mapirajStatusTekst(r.status);
        
        let ponudaHtml = '';
        if (!r.jeZahtev && r.status === 'pending' && r.cenaDonja && r.cenaGornja) {
            ponudaHtml = `<button class="btn btn-sm dugme" onclick="prikaziPonudu(${r.id})">
                            <i class="fa-solid fa-file-invoice-dollar"></i> Vidi ponudu
                        </button>`;
        } else {
            ponudaHtml = '<span class="tekst-crven">---</span>';
        }
        
        let placanjeHtml = '';
        if (!r.jeZahtev && r.status === 'confirmed') {
            if (r.placeno) {
                placanjeHtml = '<span class="verifikovan-badge"><i class="fa-solid fa-check"></i> Plaćeno</span>';
            } else {
                if (r.konacnaCena && r.konacnaCena > 0) {
                    placanjeHtml = `<button class="btn btn-sm dugme" onclick="platiDirektno(${r.id_zakazivanja}, ${r.konacnaCena})">
                                        <i class="fa-solid fa-credit-card"></i> Plati (${r.konacnaCena} EUR)
                                    </button>`;
                } else {
                    placanjeHtml = '<span class="tekst-crven">Čeka se cena</span>';
                }
            }
        } else {
            placanjeHtml = '<span class="tekst-crven">---</span>';
        }
        
        html += `<tr>
            <td class="tekst-crven">${r.majstor || 'Nepoznat'}</td>
            <td class="tekst-crven">${r.datum || 'Nepoznat'}</td>
            <td class="tekst-crven">${r.vreme || '---'}</td>
            <td class="${statusKlasa}">${statusTekst}</td>
            <td class="text-center">${ponudaHtml}</td>
            <td class="text-center">${placanjeHtml}</td>
        </tr>`;
    }
    tbody.innerHTML = html;
}

function prikaziPonudu(id) {
    const rez = sveRezervacije.find(r => r.id === id);
    if (!rez) return;
    
    prikazanaPonudaId = rez.id_zakazivanja;
    
    document.getElementById('modalCenaDonja').innerHTML = (rez.cenaDonja || 0) + ' EUR';
    document.getElementById('modalCenaGornja').innerHTML = (rez.cenaGornja || 0) + ' EUR';
    
    let trajanje = 'Prema dogovoru';
    if (rez.pocetak && rez.kraj) {
        trajanje = `${rez.pocetak} - ${rez.kraj}`;
    }
    document.getElementById('modalTrajanje').innerHTML = trajanje;
    document.getElementById('modalKomentarMajstor').innerHTML = rez.opis || 'Nema komentara';
    document.getElementById('modalPonuda').classList.add('show');
}

function zatvoriModalPonuda() {
    document.getElementById('modalPonuda').classList.remove('show');
    prikazanaPonudaId = null;
}

async function prihvatiPonudu() {
    if (!prikazanaPonudaId) return;
    
    try {
        await odgovoriKlijentNaZakazivanje(prikazanaPonudaId, true);
        alert('Ponuda je prihvaćena! Termin je potvrđen.');
        zatvoriModalPonuda();
        await ucitajRezervacije();
    } catch (error) {
        alert('Greška pri prihvatanju ponude');
    }
}

async function odbijPonudu() {
    if (!prikazanaPonudaId) return;
    
    try {
        await odgovoriKlijentNaZakazivanje(prikazanaPonudaId, false);
        alert('Ponuda je odbijena.');
        zatvoriModalPonuda();
        await ucitajRezervacije();
    } catch (error) {
        alert('Greška pri odbijanju ponude');
    }
}

function filtrirajRezervacije() {
    const status = document.getElementById('filterStatus').value;
    if (status === 'sve') {
        filtriraneRezervacije = [...sveRezervacije];
    } else {
        filtriraneRezervacije = sveRezervacije.filter(r => r.status === status);
    }
    prikaziRezervacije();
}

async function platiDirektno(zakazivanjeId, iznos) {
    try {
        const data = await platiKarticom(zakazivanjeId);
        window.location.href = data.url;
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri plaćanju: ' + error.message);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    ucitajRezervacije();
    const filterBtn = document.getElementById('filtrirajBtn');
    if (filterBtn) {
        filterBtn.addEventListener('click', filtrirajRezervacije);
    }
});