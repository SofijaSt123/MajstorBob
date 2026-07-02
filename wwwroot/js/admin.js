const tipKorisnika = localStorage.getItem('tip_korisnika');
const korisnikId = localStorage.getItem('korisnik_id');

if (tipKorisnika?.toLowerCase() !== 'admin') {
    window.location.href = 'index.html';
}

let prijave = [];
let trenutnaPrijavaId = null;
let sveKategorijeAdmin = [];

async function ucitajPrijave() {
    try {
        const data = await getPrijave();
        console.log('Učitane prijave:', data);
        
        const prijaveSaImenima = [];
        for (const p of data) {
            let imePrijavio = `Klijent #${p.id_prijavljaca}`;
            let imePrijavljeni = `Izvođač #${p.id_prijavljena_osoba}`;
            
            try {
                const klijent = await getKlijentInfoForAdmin(p.id_prijavljaca);
                if (klijent && klijent.ime) {
                    imePrijavio = `${klijent.ime} ${klijent.prezime || ''}`;
                }
            } catch (e) {
                console.warn('Ne mogu da dohvatim klijenta:', p.id_prijavljaca);
            }
            
            let imePronadjeno = false;
            
            try {
                const majstor = await getMajstorInfoForKlijent(p.id_prijavljena_osoba);
                if (majstor && majstor.ime) {
                    imePrijavljeni = `${majstor.ime} ${majstor.prezime || ''}`;
                    imePronadjeno = true;
                }
            } catch (e) {
            }
            
            if (!imePronadjeno) {
                try {
                    const firma = await getFirmaInfoForKlijent(p.id_prijavljena_osoba);
                    if (firma && firma.naziv_firme) {
                        imePrijavljeni = firma.naziv_firme;
                        imePronadjeno = true;
                    } else if (firma && firma.ime) {
                        imePrijavljeni = `${firma.ime} ${firma.prezime || ''}`;
                        imePronadjeno = true;
                    }
                } catch (e) {
                    console.warn('Ne mogu da dohvatim izvođača:', p.id_prijavljena_osoba);
                }
            }
            
            if (!imePronadjeno) {
                imePrijavljeni = `Izvođač #${p.id_prijavljena_osoba}`;
            }
            
            prijaveSaImenima.push({
                id: p.id_prijave,
                datum: p.kreirano ? new Date(p.kreirano).toLocaleDateString('sr-RS') : 'Nepoznat',
                prijavio: imePrijavio,
                prijavljeniIme: imePrijavljeni,
                razlog: p.razlog || 'Nije naveden',
                id_prijavljena_osoba: p.id_prijavljena_osoba,
                id_prijavljaca: p.id_prijavljaca
            });
        }
        
        prijave = prijaveSaImenima;
        prikaziPrijave();
    } catch (error) {
        console.error('Greška:', error);
        prijave = [];
        prikaziPrijave();
    }
}

function prikaziPrijave() {
    const tbody = document.getElementById('tabelaPrijava');
    
    if (!prijave || prijave.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">Nema nerešenih prijava</td></tr>';
        return;
    }
    
    let html = '';
    for (let p of prijave) {
        const prijavio = p.prijavio.length > 15 ? p.prijavio.substring(0, 15) + '...' : p.prijavio;
        const prijavljeni = p.prijavljeniIme.length > 15 ? p.prijavljeniIme.substring(0, 15) + '...' : p.prijavljeniIme;
        
        html += `
            <tr>
                <td class="text-center">${p.id}</td>
                <td class="tekst-crven">${prijavio}</td>
                <td class="tekst-crven">${prijavljeni}</td>
                <td class="tekst-crven">${p.razlog}</td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="prikaziDetaljePrijave(${p.id})" title="Detalji">
                        <i class="fa-solid fa-eye"></i>
                    </button>
                </td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="otvoriModalKomentar(${p.id})" title="Reši">
                        <i class="fa-solid fa-check"></i>
                    </button>
                </td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
}

function prikaziDetaljePrijave(id) {
    const prijava = prijave.find(p => p.id === id);
    if (!prijava) return;
    
    document.getElementById('modalPrijavaId').innerHTML = prijava.id;
    document.getElementById('modalPrijavaDatum').innerHTML = prijava.datum;
    document.getElementById('modalPrijavaPrijavio').innerHTML = prijava.prijavio;
    document.getElementById('modalPrijavaPrijavljeni').innerHTML = prijava.prijavljeniIme;
    document.getElementById('modalPrijavaRazlog').innerHTML = prijava.razlog;
    
    document.getElementById('modalDetaljiPrijave').classList.add('show');
}

function zatvoriModalDetaljiPrijave() {
    document.getElementById('modalDetaljiPrijave').classList.remove('show');
}

function otvoriModalKomentar(id) {
    trenutnaPrijavaId = id;
    
    const prijava = prijave.find(p => p.id === id);
    if (prijava) {
        document.getElementById('modalPrijavaIme').innerHTML = prijava.prijavljeniIme || 'Nepoznato';
        document.getElementById('modalPrijavaRazlogKratko').innerHTML = prijava.razlog || 'Nije naveden';
    }
    
    document.getElementById('komentarTekst').value = '';
    
    const select = document.getElementById('blokirajKorisnikaSelect');
    if (select) {
        select.value = 'true';
        azurirajBlokiranjeInfoSelect();
    }
    
    document.getElementById('modalKomentarPrijave').classList.add('show');
}

function zatvoriModalKomentar() {
    document.getElementById('modalKomentarPrijave').classList.remove('show');
    trenutnaPrijavaId = null;
}

function azurirajBlokiranjeInfoSelect() {
    const select = document.getElementById('blokirajKorisnikaSelect');
    const info = document.getElementById('blokiranjeInfoSelect');
    const upozorenje = document.getElementById('blokiranjeUpozorenjeSelect');
    
    if (!select || !info || !upozorenje) return;
    
    if (select.value === 'true') {
        info.textContent = 'Uključeno - korisnik će biti trajno blokiran';
        upozorenje.style.display = 'block';
    } else {
        info.textContent = 'Isključeno - korisnik NEĆE biti blokiran';
        upozorenje.style.display = 'none';
    }
}

async function posaljiObavestenjeKlijentu(prijava, blokiran, adminKomentar) {
    console.log('adminKomentar STIGAO U FUNKCIJU:', adminKomentar);  
    const komentar = adminKomentar || 'Niste uneli komentar';
    
    console.log('Šaljem obaveštenje:');
    console.log('  receiver_id:', prijava.id_prijavljaca);
    console.log('  naslov: Admin: ' + komentar);
    
    try {
        await posaljiObavestenje({
            receiver_id: prijava.id_prijavljaca,
            naslov: 'Admin: ' + komentar,
            tip_kome_saljes: null
        });
        console.log('Obaveštenje poslato');
    } catch (error) {
        console.error('Greška:', error);
    }
}

async function posaljiObavestenjeIzvodjacu(prijava) {

    const poruka = `Admin: Upozoravamo vas da ste flegovani zbog prijave korisnika. Molimo vas da poštujete pravila platforme.`;
    
    console.log('Šaljem upozorenje IZVOĐAČU:');
    console.log('receiver_id:', prijava.id_prijavljena_osoba);
    console.log('naslov:', 'Upozorenje od administratora');
    
    try {
        await posaljiObavestenje({
            receiver_id: prijava.id_prijavljena_osoba,
            naslov: poruka,
            tip_kome_saljes: null
        });
        console.log('Upozorenje poslato izvođaču');
    } catch (error) {
        console.error('Greška pri slanju upozorenja izvođaču:', error);
    }
}

async function sacuvajKomentar() {
    if (!trenutnaPrijavaId) return;
    
    const komentar = document.getElementById('komentarTekst').value.trim();
    console.log(' Komentar iz textarea: "' + komentar + '"');
    
    const blokirajSelect = document.getElementById('blokirajKorisnikaSelect');
    const blokiraj = blokirajSelect ? blokirajSelect.value === 'true' : true;
    
    if (!komentar) {
        alert('Unesite komentar!');
        return;
    }
    
    const prijava = prijave.find(p => p.id === trenutnaPrijavaId);
    if (!prijava) {
        alert('Prijava nije pronađena!');
        return;
    }
    
    try {
        await obradiPrijavu(trenutnaPrijavaId, komentar, blokiraj);
        
        await posaljiObavestenjeKlijentu(prijava, blokiraj, komentar);

        if (!blokiraj) {
            await posaljiObavestenjeIzvodjacu(prijava, komentar);
        }
        
        const poruka = blokiraj 
            ? 'Prijava je rešena i korisnik je blokiran!' 
            : 'Prijava je rešena bez blokiranja korisnika. Klijent i majstor su obavešteni.';
        alert(poruka);
        
        zatvoriModalKomentar();
        await ucitajPrijave();
        
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri rešavanju prijave: ' + error.message);
    }
}

async function ucitajOceneZaMajstora() {
    const idInput = document.getElementById('filterMajstorOceneId');
    const majstorId = idInput?.value;
    
    if (!majstorId) {
        alert('Unesite ID majstora/firme!');
        return;
    }
    
    const tbody = document.getElementById('tabelaOcena');
    tbody.innerHTML = '<tr><td colspan="5" class="text-center">Učitavanje ocena...</td></tr>';
    
    try {
        const ocene = await getOceneMajstora(majstorId);
        
        if (!ocene || ocene.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">Nema ocena za ovog majstora</td></tr>';
            return;
        }
        
        let imeMajstora = `Majstor #${majstorId}`;
        try {
            const podaci = await getMajstorInfoForKlijent(majstorId);
            if (podaci && podaci.ime) {
                imeMajstora = `${podaci.ime} ${podaci.prezime || ''}`;
            }
        } catch (e) {
            try {
                const podaci = await getFirmaInfoForKlijent(majstorId);
                if (podaci && podaci.naziv_firme) {
                    imeMajstora = podaci.naziv_firme;
                }
            } catch (e2) {}
        }
        
        prikaziOceneTabela(ocene, imeMajstora, majstorId);
        
    } catch (error) {
        console.error('Greška:', error);
        tbody.innerHTML = '<tr><td colspan="5" class="text-center tekst-crven">Greška pri učitavanju ocena</td></tr>';
    }
}

function prikaziOceneTabela(ocene, imeMajstora, majstorId) {
    const tbody = document.getElementById('tabelaOcena');
    
    if (!ocene || ocene.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center">Nema ocena za prikaz</td></tr>';
        return;
    }
    
    const oceneSaProsekom = ocene.map(o => {
        const prosek = (o.ocena_cena + o.ocena_kvaliteta + o.ocena_brzine + o.ocena_odnosa) / 4;
        return { ...o, prosek };
    });
    
    oceneSaProsekom.sort((a, b) => b.prosek - a.prosek);
    
    let html = '';
    for (let i = 0; i < oceneSaProsekom.length; i++) {
        const o = oceneSaProsekom[i];
        const idOcene = o.id_ocene || o.id;
        
        html += `
            <tr>
                <td class="text-center">${i + 1}</td>
                <td class="tekst-crven">${imeMajstora}</td>
                <td class="text-center">${o.prosek.toFixed(1)}</td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="prikaziDetaljeOcene(${i})" title="Detalji"><i class="fa-solid fa-circle-info"></i></button>
                </td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="obrisiOcenuAdmin(${idOcene})" title="Obriši"><i class="fa-solid fa-trash"></i></button>
                </td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
    
    window.trenutneOcene = oceneSaProsekom;
}

function prikaziDetaljeOcene(index) {
    const ocene = window.trenutneOcene;
    if (!ocene || !ocene[index]) return;
    
    const o = ocene[index];
    const detalji = document.getElementById('modalOcenaDetalji');
    detalji.innerHTML = `
        <div class="info-red"><span class="oznaka-polja">ID ocene:</span><span class="tekst-crven">${o.id_ocene || index + 1}</span></div>
        <div class="info-red"><span class="oznaka-polja">Cena usluge:</span><span class="tekst-crven">${o.ocena_cena}</span></div>
        <div class="info-red"><span class="oznaka-polja">Kvalitet rada:</span><span class="tekst-crven">${o.ocena_kvaliteta}</span></div>
        <div class="info-red"><span class="oznaka-polja">Brzina izvršenja:</span><span class="tekst-crven">${o.ocena_brzine}</span></div>
        <div class="info-red"><span class="oznaka-polja">Odnos prema klijentu:</span><span class="tekst-crven">${o.ocena_odnosa}</span></div>
        <div class="info-red"><span class="oznaka-polja">Prosečna ocena:</span><span class="tekst-crven">${o.prosek.toFixed(1)}</span></div>
        <div class="info-red"><span class="oznaka-polja">Recenzija:</span><span class="tekst-crven">${o.opis_recenzije || 'Nema recenzije'}</span></div>
        ${o.odgovor ? `<div class="info-red"><span class="oznaka-polja">Odgovor majstora:</span><span class="tekst-crven">${o.odgovor}</span></div>` : ''}
    `;
    document.getElementById('modalDetaljiOcene').classList.add('show');
}

function zatvoriModalDetaljiOcene() {
    document.getElementById('modalDetaljiOcene').classList.remove('show');
}

async function obrisiOcenuAdmin(ocenaId) {
    if (!confirm('Da li ste sigurni da želite da obrišete ovu ocenu?')) return;
    try {
        await obrisiOcenu(ocenaId);
        alert('Ocena je obrisana!');
        ucitajOceneZaMajstora();
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri brisanju ocene');
    }
}

document.getElementById('obavestenjeTipPrimaoca')?.addEventListener('change', function() {
    const polje = document.getElementById('obavestenjeKorisnikPolje');
    if (this.value === 'specific') {
        polje.style.display = 'block';
    } else {
        polje.style.display = 'none';
    }
});

async function posaljiObavestenjeAdmin() {
    const tipPrimaoca = document.getElementById('obavestenjeTipPrimaoca').value;
    const poruka = document.getElementById('obavestenjePoruka')?.value.trim();
    
    if (!poruka) {
        alert('Unesite poruku!');
        return;
    }
    
    let payload = {
        naslov: poruka,
        receiver_id: null,
        tip_kome_saljes: null
    };
    
    if (tipPrimaoca === 'specific') {
        const korisnikId = document.getElementById('obavestenjeKorisnikId')?.value.trim();
        if (!korisnikId || isNaN(korisnikId) || parseInt(korisnikId) <= 0) {
            alert('Unesite validan ID korisnika!');
            return;
        }
        payload.receiver_id = parseInt(korisnikId);
        payload.tip_kome_saljes = null;
    } else if (tipPrimaoca === 'klijenti') {
        payload.receiver_id = null;
        payload.tip_kome_saljes = 0;
    } else if (tipPrimaoca === 'izvodjaci') {
        payload.receiver_id = null;
        payload.tip_kome_saljes = 1;
    } else if (tipPrimaoca === 'svi') {
        payload.receiver_id = null;
        payload.tip_kome_saljes = 2;
    }
    
    try {
        await posaljiObavestenje(payload);
        alert('Obaveštenje je poslato!');
        document.getElementById('obavestenjePoruka').value = '';
        document.getElementById('obavestenjeKorisnikId').value = '';
        document.getElementById('obavestenjeTipPrimaoca').value = 'specific';
        document.getElementById('obavestenjeKorisnikPolje').style.display = 'block';
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri slanju obaveštenja: ' + error.message);
    }
}

async function ucitajGradove() {
    try {
        const gradovi = await getGradovi();
        console.log('Učitani gradovi:', gradovi);
        prikaziGradove(gradovi);
    } catch (error) {
        console.error('Greška pri učitavanju gradova:', error);
        document.getElementById('tabelaGradova').innerHTML = '<tr><td colspan="3" class="text-center tekst-crven">Greška pri učitavanju gradova</td></tr>';
    }
}

function prikaziGradove(gradovi) {
    const tbody = document.getElementById('tabelaGradova');
    
    if (!gradovi || gradovi.length === 0) {
        tbody.innerHTML = '<tr><td colspan="3" class="text-center">Nema dodatih gradova</td></tr>';
        return;
    }
    
    const sortirani = [...gradovi].sort((a, b) => a.id_grad - b.id_grad);
    
    let html = '';
    for (let g of sortirani) {
        html += `
            <tr>
                <td style="text-align: left; padding-left: 10px;">${g.id_grad}</td>
                <td class="tekst-crven">${g.naziv_grada}</td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="obrisiGradAdmin(${g.id_grad})" title="Obriši grad"><i class="fa-solid fa-trash"></i></button>
                </td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
}

async function dodajGradAdmin() {
    const input = document.getElementById('noviGradInput');
    const naziv = input.value.trim();
    
    if (!naziv) {
        alert('Unesite naziv grada!');
        return;
    }
    
    try {
        await dodajGrad(naziv);
        alert('Grad je uspešno dodat!');
        input.value = '';
        await ucitajGradove();
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri dodavanju grada');
    }
}

async function obrisiGradAdmin(id) {
    if (!confirm('Da li ste sigurni da želite da obrišete ovaj grad?')) return;
    try {
        await obrisiGrad(id);
        alert('Grad je uspešno obrisan!');
        await ucitajGradove();
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri brisanju grada');
    }
}

async function ucitajKategorijeAdmin() {
    try {
        sveKategorijeAdmin = await getKategorije();
        console.log('Učitane kategorije:', sveKategorijeAdmin);
        prikaziKategorije(sveKategorijeAdmin);
        napuniRoditeljSelect(sveKategorijeAdmin);
    } catch (error) {
        console.error('Greška pri učitavanju kategorija:', error);
        document.getElementById('tabelaKategorija').innerHTML = '<tr><td colspan="3" class="text-center tekst-crven">Greška pri učitavanju kategorija</td></tr>';
    }
}

function prikaziKategorije(kategorije) {
    const tbody = document.getElementById('tabelaKategorija');
    
    if (!kategorije || kategorije.length === 0) {
        tbody.innerHTML = '<tr><td colspan="3" class="text-center">Nema dodatih kategorija</td></tr>';
        return;
    }
    
    const sortirani = [...kategorije].sort((a, b) => a.id_kategorije - b.id_kategorije);
    
    let html = '';
    for (let k of sortirani) {
        const roditeljNaziv = kategorije.find(r => r.id_kategorije === k.id_roditelja)?.naziv_kategorije || 'Bez roditelja';
        html += `
            <tr>
                <td style="text-align: left; padding-left: 10px;">${k.id_kategorije}</td>
                <td class="tekst-crven">${k.naziv_kategorije}</td>
                <td class="tekst-crven">${roditeljNaziv}</td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
}

function napuniRoditeljSelect(kategorije) {
    const select = document.getElementById('roditeljKategorijaSelect');
    select.innerHTML = '<option value="">Bez roditelja</option>';
    
    const glavne = kategorije.filter(k => k.id_roditelja === null);
    for (let k of glavne) {
        const option = document.createElement('option');
        option.value = k.id_kategorije;
        option.textContent = k.naziv_kategorije;
        option.style.fontWeight = 'bold';
        select.appendChild(option);
        
        const deca = kategorije.filter(d => d.id_roditelja === k.id_kategorije);
        for (let d of deca) {
            const optDeca = document.createElement('option');
            optDeca.value = d.id_kategorije;
            optDeca.textContent = '    ' + d.naziv_kategorije;
            select.appendChild(optDeca);
        }
    }
}

async function dodajKategorijuAdmin() {
    const input = document.getElementById('novaKategorijaInput');
    const naziv = input.value.trim();
    const roditeljId = document.getElementById('roditeljKategorijaSelect').value;
    
    if (!naziv) {
        alert('Unesite naziv kategorije!');
        return;
    }
    
    try {
        await dodajKategoriju(naziv, roditeljId);
        
        alert('Kategorija je uspešno dodata!');
        input.value = '';
        document.getElementById('roditeljKategorijaSelect').value = '';
        await ucitajKategorijeAdmin();
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri dodavanju kategorije: ' + error.message);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    ucitajPrijave();
    ucitajGradove();
    ucitajKategorijeAdmin();
    
    document.getElementById('posaljiObavestenjeBtn')?.addEventListener('click', posaljiObavestenjeAdmin);
    document.getElementById('prikaziOceneBtn')?.addEventListener('click', ucitajOceneZaMajstora);
    document.getElementById('dodajGradBtn')?.addEventListener('click', dodajGradAdmin);
    document.getElementById('noviGradInput')?.addEventListener('keyup', function(e) {
        if (e.key === 'Enter') dodajGradAdmin();
    });
    document.getElementById('dodajKategorijuBtn')?.addEventListener('click', dodajKategorijuAdmin);
    document.getElementById('novaKategorijaInput')?.addEventListener('keyup', function(e) {
        if (e.key === 'Enter') dodajKategorijuAdmin();
    });
    
    const select = document.getElementById('blokirajKorisnikaSelect');
    if (select) {
        select.addEventListener('change', azurirajBlokiranjeInfoSelect);
    }
});