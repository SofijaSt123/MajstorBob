const tipKorisnika = localStorage.getItem('tip_korisnika');
if (tipKorisnika?.toLowerCase() !== 'majstor' && tipKorisnika?.toLowerCase() !== 'firma') {
    window.location.href = 'index.html';
}

let sviTermini = [];
let filtriraniTermini = [];
let trenutniTerminId = null;
let trenutniZahtevId = null;
let trenutniTerminIdZaCenu = null;
let trenutniTerminPodaci = null;

async function proveriStripeStatusZaTermine() {
    const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
    if (tipLower !== 'majstor' && tipLower !== 'firma') {
        return false;
    }
    
    try {
        let podaci;
        if (tipLower === 'majstor') {
            podaci = await getMajstor();
        } else {
            podaci = await getFirma();
        }
        if (podaci && podaci.stripe_num) {
            return true;
        }
    } catch (e) {
        console.warn('Ne mogu da proverim Stripe status:', e);
    }
    return false;
}

async function dohvatiImeKlijenta(klijentId) {
    try {
        const klijent = await getKlijentInfoForAdmin(klijentId);
        if (klijent && klijent.ime) {
            return `${klijent.ime} ${klijent.prezime || ''}`;
        }
    } catch (e) {
        console.warn(`Ne mogu da dohvatim klijenta ${klijentId}`);
    }
    return `Klijent #${klijentId}`;
}

async function ucitajTermine() {
    const tbody = document.getElementById('tabelaTermina');
    tbody.innerHTML = '<tr><td colspan="7" class="text-center">Učitavanje termina...</td></tr>';
    
    try {
        const zahtevi = await getZahtevi();
        const zakazivanja = await getZakazivanja();
        let sviTerminiTemp = [];
        
        for (const z of zahtevi) {
            const statusBroj = Number(z.status);
            if (statusBroj !== 0) continue;  
            
            const imeKlijenta = await dohvatiImeKlijenta(z.id_klijenta);
            
            sviTerminiTemp.push({
                id: z.id_zahteva,
                id_zahteva: z.id_zahteva,
                id_zakazivanja: null,
                klijent: imeKlijenta,
                datum: z.datum,
                vreme: z.vreme,
                opis: z.opis_radova || '',      
                adresa: z.adresa || '',
                status: mapirajStatus(z.status),
                cenaDonja: null,
                cenaGornja: null,
                konacnaCena: null,
                placeno: false
            });
        }
        
        for (const z of zakazivanja) {
            const statusBroj = Number(z.status);
            if (statusBroj !== 0 && statusBroj !== 2) continue; 
            
            let imeKlijenta = 'Nepoznati klijent';
            let opisKlijenta = '';
            let adresaKlijenta = '';
            
            const zahtev = zahtevi.find(req => req.id_zahteva === z.id_zahteva);
            if (zahtev) {
                if (zahtev.id_klijenta) {
                    imeKlijenta = await dohvatiImeKlijenta(zahtev.id_klijenta);
                }
                opisKlijenta = zahtev.opis_radova || '';
                adresaKlijenta = zahtev.adresa || '';
            }
            
            let status = mapirajStatus(z.status);
            
            sviTerminiTemp.push({
                id: z.id_zakazivanja,
                id_zahteva: z.id_zahteva,
                id_zakazivanja: z.id_zakazivanja,
                klijent: imeKlijenta,
                datum: z.datum,
                vreme: z.pocetak || '',
                opis: opisKlijenta,              
                adresa: adresaKlijenta,          
                status: status,
                cenaDonja: z.cena_donja || 0,
                cenaGornja: z.cena_gornja || 0,
                konacnaCena: z.konacna_cena || null,
                placeno: z.da_li_je_placeno || false,
                majstorKomentar: z.opis || ''    
            });
        }
        
        sviTerminiTemp.sort((a, b) => new Date(b.datum) - new Date(a.datum));
        
        sviTermini = sviTerminiTemp;
        filtriraniTermini = [...sviTermini];
        prikaziTermine();
        
    } catch (error) {
        console.error('Greška:', error);
        tbody.innerHTML = '<tr><td colspan="7" class="text-center tekst-crven">Greška pri učitavanju termina</td></tr>';
    }
}

function mapirajStatus(status) {
    const s = Number(status);
    switch(s) {
        case 0: return 'pending';
        case 1: return 'rejected';
        case 2: return 'confirmed';
        default: return 'pending';
    }
}

function mapirajStatusTekst(status) {
    switch(status) {
        case 'pending': return '<i class="fa-solid fa-clock"></i> Na čekanju';
        case 'confirmed': return '<i class="fa-solid fa-circle-check"></i> Potvrđeno';
        case 'rejected': return '<i class="fa-solid fa-circle-xmark"></i> Odbijeno';
        default: return '<i class="fa-solid fa-question"></i> Nepoznat';
    }
}

function mapirajStatusKlasu(status) {
    switch(status) {
        case 'pending': return 'status-cekanje';
        case 'confirmed': return 'status-potvrdjeno';
        case 'rejected': return 'status-odbijeno';
        default: return '';
    }
}

function prikaziTermine() {
    const tbody = document.getElementById('tabelaTermina');
    
    if (!filtriraniTermini || filtriraniTermini.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center">Nema termina za prikaz</td></tr>';
        return;
    }
    
    let html = '';
    for (let t of filtriraniTermini) {
        const statusKlasa = mapirajStatusKlasu(t.status);
        const statusTekst = mapirajStatusTekst(t.status);
        
        html += `
            <tr>
                <td class="tekst-crven">${t.klijent}</td>
                <td class="tekst-crven">${t.datum}</td>
                <td class="tekst-crven">${t.vreme}</td>
                <td class="tekst-crven" style="max-width: 150px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; cursor: pointer;" 
                    title="${t.opis || 'Nema opisa'}" 
                    onclick="prikaziDetaljeTermina(${t.id})">
                    ${t.opis || 'Nema opisa'}
                </td>
                <td class="tekst-crven">${t.adresa || 'Nije uneta'}</td>
                <td class="${statusKlasa}">${statusTekst}</td>
                <td class="akcije-red">
                    <div style="display: flex; gap: 6px; flex-wrap: wrap; align-items: center;">
                    ${t.status === 'pending' ? `
                        ${!t.id_zakazivanja ? `
                            <button class="btn btn-sm dugme" onclick="otvoriModalPrihvati(${t.id})"><i class="fa-solid fa-check"></i> Prihvati</button>
                            <button class="btn btn-sm dugme" onclick="odbijTermin(${t.id})"><i class="fa-solid fa-xmark"></i> Odbij</button>
                        ` : `
                            <span class="tekst-crven"><i class="fa-solid fa-clock"></i> Čeka klijenta</span>
                        `}
                    ` : ''}
                    ${t.status === 'confirmed' ? `
                        ${t.placeno ? `
                            <span class="verifikovan-badge"><i class="fa-solid fa-check"></i> Plaćeno</span>
                        ` : `
                            ${t.konacnaCena && t.konacnaCena > 0 ? `
                                <span class="tekst-crven"><i class="fa-solid fa-clock"></i> Čeka se plaćanje</span>
                            ` : `
                                <button class="btn btn-sm dugme" onclick="otvoriModalKonacnaCena(${t.id})">
                                    <i class="fa-solid fa-coins"></i> Postavi cenu
                                </button>
                            `}
                        `}
                    ` : ''}
                </td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
}

function prikaziDetaljeTermina(id) {
    const termin = sviTermini.find(t => t.id === id);
    if (!termin) return;
    
    document.getElementById('modalKlijent').innerHTML = termin.klijent;
    document.getElementById('modalDatum').innerHTML = termin.datum;
    document.getElementById('modalVreme').innerHTML = termin.vreme;
    document.getElementById('modalOpis').innerHTML = termin.opis || 'Nema opisa';
    document.getElementById('modalAdresa').innerHTML = termin.adresa || 'Nije uneta';
    
    document.getElementById('modalDetaljiTermina').classList.add('show');
}

function zatvoriModalDetalji() {
    document.getElementById('modalDetaljiTermina').classList.remove('show');
}

function filtrirajTermine() {
    const status = document.getElementById('filterStatus').value;
    if (status === 'sve') {
        filtriraniTermini = [...sviTermini];
    } else {
        filtriraniTermini = sviTermini.filter(t => t.status === status);
    }
    prikaziTermine();
}

function otvoriModalPrihvati(id) {
    const termin = sviTermini.find(t => t.id === id);
    if (!termin) {
        alert('Termin nije pronađen!');
        return;
    }
    
    trenutniZahtevId = termin.id_zahteva;
    
    document.getElementById('cenaDonja').value = '';
    document.getElementById('cenaGornja').value = '';
    document.getElementById('komentarMajstor').value = '';
    document.getElementById('datumIzvodjenja').value = '';
    document.getElementById('pocetakRadova').value = '08:00';
    document.getElementById('krajRadova').value = '16:00';
    
    document.getElementById('modalPrihvati').classList.add('show');
}

function zatvoriModalPrihvati() {
    document.getElementById('modalPrihvati').classList.remove('show');
    trenutniZahtevId = null;
}

async function posaljiPonudu() {
    const cenaDonja = document.getElementById('cenaDonja').value;
    const cenaGornja = document.getElementById('cenaGornja').value;
    const komentar = document.getElementById('komentarMajstor').value;
    const datum = document.getElementById('datumIzvodjenja').value;
    let pocetak = document.getElementById('pocetakRadova').value;
    let kraj = document.getElementById('krajRadova').value;
    
    if (!cenaDonja || !cenaGornja) {
        alert('Morate uneti donju i gornju granicu cene!');
        return;
    }
    
    if (parseInt(cenaDonja) > parseInt(cenaGornja)) {
        alert('Donja granica cene ne može biti veća od gornje!');
        return;
    }
    
    if (!datum) {
        alert('Morate izabrati datum izvođenja!');
        return;
    }
    
    if (!pocetak || !kraj) {
        alert('Morate uneti vreme početka i kraja!');
        return;
    }
    
    if (pocetak && pocetak.split(':').length === 2) {
        pocetak = pocetak + ':00';
    }
    if (kraj && kraj.split(':').length === 2) {
        kraj = kraj + ':00';
    }
    
    const podaci = {
        id_zahteva: trenutniZahtevId,
        cena_donja: parseInt(cenaDonja),
        cena_gornja: parseInt(cenaGornja),
        pocetak: pocetak,
        kraj: kraj,
        datum: datum,
        opis: komentar || 'Ponuda majstora'
    };
    
    try {
        await odgovoriNaZahtev(trenutniZahtevId, true, podaci);
        alert('Ponuda je poslata klijentu na potvrdu!');
        zatvoriModalPrihvati();
        await ucitajTermine();
    } catch (error) {
        alert('Greška pri slanju ponude. Pokušajte ponovo.');
    }
}

async function odbijTermin(id) {
    if (!confirm('Da li ste sigurni da želite da odbijete ovaj termin?')) {
        return;
    }
    
    const termin = sviTermini.find(t => t.id === id);
    if (!termin) {
        alert('Termin nije pronađen!');
        return;
    }
    
    try {
        const podaci = {
            id_zahteva: termin.id_zahteva,
            cena_donja: 0,
            cena_gornja: 0,
            pocetak: "00:00:00",
            kraj: "00:00:00",
            datum: new Date().toISOString().split('T')[0],
            opis: "Odbijen od strane majstora"
        };
        
        await odgovoriNaZahtev(termin.id_zahteva, false, podaci);
        alert('Termin je odbijen!');
        await ucitajTermine();
    } catch (error) {
        alert('Greška pri odbijanju termina');
    }
}

async function otvoriModalKonacnaCena(id) {
    const termin = sviTermini.find(t => t.id === id);
    if (!termin) {
        alert('Termin nije pronađen!');
        return;
    }
    
    let idZakazivanja = termin.id_zakazivanja;
    
    if (!idZakazivanja) {
        try {
            const zakazivanja = await getZakazivanja();
            const zakaz = zakazivanja.find(z => z.id_zahteva === termin.id_zahteva);
            if (zakaz) {
                idZakazivanja = zakaz.id_zakazivanja;
            }
        } catch (e) {}
    }
    
    if (!idZakazivanja) {
        alert('ID zakazivanja nije pronađen! Obratite se administratoru.');
        return;
    }
    
    trenutniTerminIdZaCenu = idZakazivanja;
    trenutniTerminPodaci = termin;
    
    document.getElementById('modalKonacnaKlijent').innerHTML = termin.klijent || 'Nepoznat';
    document.getElementById('konacnaCenaInput').value = '';
    
    const select = document.getElementById('nacinPlacanjaSelect');
    const imaStripe = await proveriStripeStatusZaTermine();
    
    if (!imaStripe) {
        select.innerHTML = `
            <option value="kes">Keš (odmah plaćeno)</option>
            <option value="kartica" disabled>Kartica (nije povezan Stripe nalog)</option>
        `;
        select.value = 'kes';
    } else {
        select.innerHTML = `
            <option value="kes">Keš (odmah plaćeno)</option>
            <option value="kartica">Kartica (klijent plaća)</option>
        `;
    }
    
    document.getElementById('modalKonacnaCena').classList.add('show');
}

function zatvoriModalKonacnaCena() {
    document.getElementById('modalKonacnaCena').classList.remove('show');
    trenutniTerminIdZaCenu = null;
    trenutniTerminPodaci = null;
}

async function sacuvajKonacnuCenu() {
    const cena = document.getElementById('konacnaCenaInput').value.trim();
    const nacinPlacanja = document.getElementById('nacinPlacanjaSelect').value;
    
    if (!cena) {
        alert('Unesite konacnu cenu!');
        return;
    }
    
    const cenaBroj = parseInt(cena);
    if (isNaN(cenaBroj) || cenaBroj <= 0) {
        alert('Unesite validnu cenu (broj veci od 0)!');
        return;
    }
    
    if (trenutniTerminPodaci && trenutniTerminPodaci.cenaGornja) {
        const maxCena = trenutniTerminPodaci.cenaGornja * 1.3;
        if (cenaBroj > maxCena) {
            alert(`Cena ne sme biti veca od ${Math.round(maxCena)} EUR (30% iznad gornje granice)`);
            return;
        }
    }
    
    try {
        if (nacinPlacanja === 'kes') {
            await platiKesom(trenutniTerminIdZaCenu, cenaBroj);
            alert('Konacna cena je postavljena i placanje je izvrseno!');
        } else {
            await setKonacnaCena(trenutniTerminIdZaCenu, cenaBroj);
            alert('Konacna cena je postavljena! Klijent ce moci da plati karticom.');
        }
        
        zatvoriModalKonacnaCena();
        await ucitajTermine();
        
    } catch (error) {
        console.error('Greska:', error);
        alert('Greska pri postavljanju konacne cene: ' + error.message);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    ucitajTermine();
    const filterBtn = document.getElementById('filtrirajBtn');
    if (filterBtn) {
        filterBtn.addEventListener('click', filtrirajTermine);
    }
});