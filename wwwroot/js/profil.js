function getAvatarUrl(ime, prezime, postojeciUrl) {
    if (!ime || !prezime) {
        return 'https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=150&name=?';
    }
    if (postojeciUrl && postojeciUrl !== "" && !postojeciUrl.startsWith('data:image')) {
        return postojeciUrl;
    }
    const inicijali = ime.charAt(0) + prezime.charAt(0);
    return `https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=150&name=${inicijali}`;
}

let trenutniKorisnik = null;
let stareKategorijeIds = new Set();

async function proveriStripeStatusZaMajstora() {
    const stripePolje = document.getElementById('stripePolje');
    const statusSpan = document.getElementById('prikazStripeStatus');
    const stripeBtn = document.getElementById('stripeBtn');
    
    stripePolje.style.display = 'block';
    
    try {
        const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
        const token = localStorage.getItem('token');
        const decoded = parseJwt(token);
        const korisnikId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'] || 
                            decoded['sid'] || 
                            decoded['nameid'] ||
                            localStorage.getItem('korisnik_id');
        
        if (!korisnikId) {
            console.error('Nema ID za Stripe proveru');
            return;
        }
        
        let podaci;
        if (tipLower === 'majstor') {
            podaci = await getMajstorInfoForKlijent(korisnikId);
        } else if (tipLower === 'firma') {
            podaci = await getFirmaInfoForKlijent(korisnikId);
        } else {
            stripePolje.style.display = 'none';
            return;
        }
        
        console.log('Stripe podaci:', podaci);
        console.log('stripe_num:', podaci?.stripe_num);
        
        if (podaci && podaci.stripe_num) {
            statusSpan.innerHTML = '<span class="verifikovan-badge"><i class="fa-solid fa-check"></i> Povezan</span>';
            stripeBtn.style.display = 'none';
        } else {
            statusSpan.innerHTML = '<span class="neverifikovan-badge"><i class="fa-solid fa-xmark"></i> Nije povezan</span>';
            stripeBtn.style.display = 'inline-block';
            stripeBtn.onclick = async function() {
                const btn = this;
                btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
                btn.disabled = true;
                try {
                    const data = await napraviStripeNalog();
                    window.location.href = data.onboardingUrl;
                } catch (error) {
                    alert('Greška: ' + error.message);
                    btn.innerHTML = '<i class="fa-brands fa-stripe"></i> Poveži';
                    btn.disabled = false;
                }
            };
        }
    } catch (e) {
        console.warn('Ne mogu da proverim Stripe status:', e);
        statusSpan.innerHTML = '<span class="neverifikovan-badge"><i class="fa-solid fa-xmark"></i> Greška</span>';
        stripeBtn.style.display = 'none';
    }
}


let gradoviRadaIzmenaData = {};

async function ucitajGradoveRadaIzmena(containerId) {
    const container = document.getElementById(containerId);
    const errorDiv = document.getElementById('gradoviRadaErrorIzmena');
    
    if (errorDiv) errorDiv.style.display = 'none';
    container.innerHTML = '<div class="text-center tekst-crven small p-2">Učitavanje...</div>';
    
    try {
        const sviGradovi = await getGradovi();
        
        let mojiGradovi = [];
        try {
            mojiGradovi = await getGradoviRada();
        } catch (e) {
            console.warn('Nema dodatih gradova rada:', e);
        }
        
        const mojiGradoviIds = new Set();
        const mojiGradoviMap = {};
        if (mojiGradovi && mojiGradovi.length > 0) {
            for (let g of mojiGradovi) {
                mojiGradoviMap[g.id_grada] = {
                    id_grad_rada: g.id_grad_rada,
                    zona_rada: g.zona_rada || false,
                    doplata: g.doplata || 0
                };
                mojiGradoviIds.add(g.id_grada);
            }
        }
        
        gradoviRadaIzmenaData = {
            sviGradovi: sviGradovi,
            mojiGradoviMap: mojiGradoviMap,
            mojiGradoviIds: mojiGradoviIds
        };
        
        let html = '';
        const dodatiGradovi = sviGradovi.filter(g => mojiGradoviIds.has(g.id_grad));
        
        html += `
            <div class="gradovi-rada-sekcija">
                <div class="sekcija-naslov">
                    <h6 class="tekst-crven mb-0"><i class="fa-solid fa-check-circle"></i> Dodati gradovi</h6>
                </div>
                <div class="gradovi-rada-lista">
        `;
        
        if (dodatiGradovi.length > 0) {
            for (let g of dodatiGradovi) {
                const podaci = mojiGradoviMap[g.id_grad];
                const doplata = podaci.doplata || 0;
                const zonaRada = podaci.zona_rada ? 'Da' : 'Ne';
                
                html += `
                    <div class="grad-rada-stavka-dodat" data-grad-rada-id="${podaci.id_grad_rada}">
                        <span class="naziv-grada">
                            <i class="fa-solid fa-location-dot ikonica-crvena"></i> 
                            ${g.naziv_grada || 'Nepoznat grad'}
                        </span>
                        <span class="doplata-info">
                            Doplata: ${zonaRada} ${doplata > 0 ? `(${doplata} EUR)` : ''}
                        </span>
                        <button class="btn btn-sm dugme obrisi-btn" onclick="obrisiGradRadaIzProfilaIzmena(${podaci.id_grad_rada})">
                            <i class="fa-solid fa-trash"></i> Obriši
                        </button>
                    </div>
                `;
            }
        } else {
            html += `
                <div class="gradovi-rada-info">
                    <i class="fa-solid fa-info-circle"></i> Trenutno nemate dodatih gradova rada.
                </div>
            `;
        }
        
        html += `
                </div>
            </div>
        `;
        
        const ostaliGradovi = sviGradovi.filter(g => !mojiGradoviIds.has(g.id_grad));
        
        if (ostaliGradovi.length > 0) {
            html += `
                <div class="gradovi-rada-sekcija mt-3">
                    <div class="sekcija-naslov">
                        <h6 class="tekst-crven mb-0"><i class="fa-solid fa-plus-circle"></i> Dodaj grad</h6>
                    </div>
                    <div class="gradovi-rada-lista">
            `;
            
            for (let g of ostaliGradovi) {
                html += `
                    <div class="grad-rada-stavka" data-id="${g.id_grad}">
                        <span class="naziv-grada">
                            <i class="fa-solid fa-location-dot ikonica-crvena"></i> 
                            ${g.naziv_grada || 'Nepoznat grad'}
                        </span>
                        
                        <div class="grad-opcije" id="grad_opcije_izmena_${g.id_grad}">
                            <div class="doplata-wrapper">
                                <label class="oznaka-polja small">Doplata:</label>
                                <select class="form-select form-select-sm" id="doplata_select_izmena_${g.id_grad}" 
                                        onchange="onDoplataSelectIzmena(${g.id_grad})">
                                    <option value="false" selected>Ne</option>
                                    <option value="true">Da</option>
                                </select>
                            </div>
                            <div class="doplata-iznos-wrapper" id="doplata_iznos_wrapper_izmena_${g.id_grad}">
                                <span class="oznaka-eur small">EUR</span>
                                <input type="number" class="form-control form-control-sm" 
                                    id="doplata_iznos_izmena_${g.id_grad}" 
                                    value="0" min="0" step="10" placeholder="0">
                            </div>
                            <button class="btn btn-sm dugme dodaj-btn" onclick="dodajGradRadaIzProfilaIzmena(${g.id_grad})">
                                <i class="fa-solid fa-plus"></i> Dodaj
                            </button>
                        </div>
                    </div>
                `;
            }
            
            html += `
                    </div>
                </div>
            `;
        } else {
            html += `
                <div class="gradovi-rada-info mt-3" style="background: #FFE6B3;">
                    <i class="fa-solid fa-check-circle"></i> Svi gradovi su već dodati!
                </div>
            `;
        }
        
        container.innerHTML = html;
        
    } catch (error) {
        console.error('Greška pri učitavanju gradova:', error);
        container.innerHTML = `<div class="text-center tekst-crven small p-2">Greška: ${error.message}</div>`;
    }
}

function onDoplataSelectIzmena(idGrada) {
    const select = document.getElementById(`doplata_select_izmena_${idGrada}`);
    const iznosWrapper = document.getElementById(`doplata_iznos_wrapper_izmena_${idGrada}`);
    const iznosInput = document.getElementById(`doplata_iznos_izmena_${idGrada}`);
    
    if (select.value === 'true') {
        iznosWrapper.style.display = 'flex';
        if (!iznosInput.value || iznosInput.value === '0') {
            iznosInput.value = '10';
        }
    } else {
        iznosWrapper.style.display = 'none';
        iznosInput.value = '0';
    }
}

async function dodajGradRadaIzProfilaIzmena(idGrada) {
    const errorDiv = document.getElementById('gradoviRadaErrorIzmena');
    errorDiv.style.display = 'none';
    
    const selectDoplata = document.getElementById(`doplata_select_izmena_${idGrada}`);
    const iznosInput = document.getElementById(`doplata_iznos_izmena_${idGrada}`);
    const opcije = document.getElementById(`grad_opcije_izmena_${idGrada}`);
    
    const zonaRada = selectDoplata?.value === 'true';
    let doplata = 0;
    if (zonaRada) {
        doplata = parseInt(iznosInput?.value) || 0;
    }
    
    if (doplata < 0) {
        errorDiv.textContent = 'Doplata ne može biti negativna!';
        errorDiv.style.display = 'block';
        return;
    }
    
    const btn = opcije?.querySelector('button');
    if (btn) {
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
        btn.disabled = true;
    }
    
    try {
        await dodajGradRada({
            id_grada: idGrada,
            zona_rada: zonaRada,
            doplata: doplata
        });
        
        alert('Grad je uspešno dodat!');
        
        if (selectDoplata) selectDoplata.value = 'false';
        const iznosWrapper = document.getElementById(`doplata_iznos_wrapper_izmena_${idGrada}`);
        if (iznosWrapper) iznosWrapper.style.display = 'none';
        if (iznosInput) iznosInput.value = '0';
        if (btn) {
            btn.innerHTML = '<i class="fa-solid fa-plus"></i> Dodaj';
            btn.disabled = false;
        }
        
        await ucitajGradoveRadaIzmena('gradoviRadaContainerIzmena');
        
    } catch (error) {
        console.error('Greška:', error);
        errorDiv.textContent = `Greška: ${error.message}`;
        errorDiv.style.display = 'block';
        
        if (btn) {
            btn.innerHTML = '<i class="fa-solid fa-plus"></i> Dodaj';
            btn.disabled = false;
        }
    }
}

async function obrisiGradRadaIzProfilaIzmena(idGradRada) {
    if (!confirm('Da li ste sigurni da želite da obrišete ovaj grad rada?')) {
        return;
    }
    
    const errorDiv = document.getElementById('gradoviRadaErrorIzmena');
    errorDiv.style.display = 'none';
    
    try {
        await deleteGradRada(idGradRada);
        await ucitajGradoveRadaIzmena('gradoviRadaContainerIzmena');
    } catch (error) {
        console.error('Greška pri brisanju:', error);
        errorDiv.textContent = `Greška pri brisanju: ${error.message}`;
        errorDiv.style.display = 'block';
    }
}

async function ucitajProfil() {
    const token = localStorage.getItem('token');
    console.log('Token:', token);
    
    if (!token) {
        console.log('NEMA TOKENA!');
        window.location.href = 'login.html';
        return;
    }
    
    const decoded = parseJwt(token);
    console.log('Decoded token:', decoded);
    
    const korisnikId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'] || 
                        decoded['sid'] || 
                        decoded['nameid'] ||
                        localStorage.getItem('korisnik_id');
    
    console.log('Korisnik ID:', korisnikId);
    
    if (!korisnikId) {
        console.error('Nema ID u tokenu');
        window.location.href = 'login.html';
        return;
    }
    
    document.getElementById('imePrezime').innerHTML = 'Učitavanje...';
    document.getElementById('prikazEmail').innerHTML = 'Učitavanje...';
    document.getElementById('tipNaloga').innerHTML = 'Učitavanje...';
    document.getElementById('prikazTip').innerHTML = 'Učitavanje...';
    document.getElementById('prikazTelefon').innerHTML = 'Učitavanje...';
    document.getElementById('prikazDatum').innerHTML = 'Učitavanje...';
    document.getElementById('prikazRadnoVreme').innerHTML = 'Učitavanje...';
    
    try {
        const tipKorisnika = localStorage.getItem('tip_korisnika');
        const tipLower = tipKorisnika?.toLowerCase();
        
        if (!tipKorisnika) {
            console.log('Nema tipa u localStorage');
            window.location.href = 'login.html';
            return;
        }
        
        console.log('Tip korisnika iz localStorage:', tipKorisnika);
        
        let korisnik;
        let profil = null;
        
        if (tipLower === 'klijent') {
            console.log('Pozivam getKlijent()');
            korisnik = await getKlijent();
            console.log('Klijent podaci:', korisnik);
        } else if (tipLower === 'majstor') {
            console.log('Pozivam InfoMajstor() sa ID:', korisnikId);
            korisnik = await getMajstor(korisnikId);
            console.log('Majstor podaci:', korisnik);
        } else if (tipLower === 'firma') {
            console.log('Pozivam InfoFirma() sa ID:', korisnikId);
            korisnik = await getFirma(korisnikId);
            console.log('Firma podaci:', korisnik);
        } else {
            console.error('Nepoznat tip korisnika:', tipKorisnika);
            window.location.href = 'login.html';
            return;
        }
        
        if (!korisnik) {
            console.error('Nema podataka o korisniku!');
            document.getElementById('imePrezime').innerHTML = 'Greška pri učitavanju';
            return;
        }
        
        let imePrezime = '';
        if (tipLower === 'firma' && korisnik.naziv_firme) {
            imePrezime = korisnik.naziv_firme;
        } else {
            imePrezime = `${korisnik.ime || ''} ${korisnik.prezime || ''}`.trim() || 'Nepoznato';
        }
        document.getElementById('imePrezime').innerHTML = imePrezime;
        document.getElementById('prikazEmail').innerHTML = korisnik.email || '';
        document.getElementById('prikazTelefon').innerHTML = korisnik.telefon || 'Nije uneto';
        
        const datum = korisnik.datum_registacije || korisnik.datum_registracije || null;
        document.getElementById('prikazDatum').innerHTML = datum ? datum.split('T')[0] : 'Nepoznat';
        
        document.getElementById('tipNaloga').innerHTML = tipKorisnika;
        document.getElementById('prikazTip').innerHTML = tipKorisnika;
        
        const avatarElement = document.getElementById('profilnaSlika');
        const inicijali = (korisnik.ime ? korisnik.ime.charAt(0) : '') + (korisnik.prezime ? korisnik.prezime.charAt(0) : '');
            avatarElement.src = `https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=150&name=${inicijali || '?'}`;

            try {
                const slikaBase64 = await getSlika();
                    if (slikaBase64) {
                        avatarElement.src = `data:image/jpeg;base64,${slikaBase64}`;
                }
        } catch (e) {
            console.log('Nema profilne slike, koristim avatar:', e);
            }

        const promeniSlikuBtn = document.getElementById('promeniSlikuBtn');
        if (promeniSlikuBtn) {
            promeniSlikuBtn.style.display = 'block';
            promeniSlikuBtn.onclick = otvoriModalSliku;
        }
        
        trenutniKorisnik = {
            id: korisnikId,
            ime: korisnik.ime,
            prezime: korisnik.prezime,
            email: korisnik.email,
            telefon: korisnik.telefon,
            //profilna_slika: korisnik.profilna_slika || '',
            grad: korisnik.grad || '',
            tip_korisnika: tipKorisnika,
            verifikovan: korisnik.verifikovan || false,
            datum_registracije: korisnik.datum_registacije
        };
        
        const verifikovanBadge = document.getElementById('verifikovanBadge');
        const verifikujBtn = document.getElementById('verifikujBtn');
        if (verifikovanBadge) {
            if (tipLower === 'majstor' || tipLower === 'firma') {
                verifikovanBadge.style.display = 'block';
                if (korisnik.verifikovan) {
                    verifikovanBadge.innerHTML = '<span class="verifikovan-badge"><i class="fa-solid fa-check"></i> Verifikovan</span>';
                    if (verifikujBtn) verifikujBtn.style.display = 'none';
                } else {
                    verifikovanBadge.innerHTML = '<span class="neverifikovan-badge"><i class="fa-solid fa-xmark"></i> Neverifikovan</span>';
                    if (verifikujBtn) {
                        verifikujBtn.style.display = 'block';
                        verifikujBtn.onclick = otvoriModalVerifikacija;
                    }
                }
            } else {
                verifikovanBadge.style.display = 'none';
                if (verifikujBtn) verifikujBtn.style.display = 'none';
            }
        }
        
        const majstorPolja = document.getElementById('majstorPolja');
        const firmaDodatno = document.getElementById('firmaDodatno');
        const tipMajstorFirma = (tipLower === 'majstor' || tipLower === 'firma');
        
        if (tipMajstorFirma) {
            majstorPolja.style.display = 'block';
            
            const radnoVremePodaci = profil || korisnik;
            const radnoVremeStr = radnoVremePodaci.pocetak_radnog_vremena && radnoVremePodaci.kraj_radnog_vremena 
                ? `${radnoVremePodaci.pocetak_radnog_vremena} - ${radnoVremePodaci.kraj_radnog_vremena}` 
                : 'Nije uneto';
            document.getElementById('prikazRadnoVreme').innerHTML = radnoVremeStr;
            
            if (tipLower === 'firma') {
                firmaDodatno.style.display = 'block';
                document.getElementById('prikazBrRadnikaUk').innerHTML = (profil || korisnik).broj_ukupnih_radnika || 0;
                document.getElementById('prikazBrRadnikaDost').innerHTML = (profil || korisnik).broj_dostupnih_radnika || 0;
                const vlasnikRed = document.getElementById('firmaVlasnikRed');
                const vlasnikSpan = document.getElementById('prikazVlasnik');
                    if (vlasnikRed && vlasnikSpan) {
                        vlasnikRed.style.display = 'block';
                        vlasnikSpan.innerHTML = `${korisnik.ime || ''} ${korisnik.prezime || ''}`.trim() || 'Nije uneto';
                    }
            } else {
                firmaDodatno.style.display = 'none';
            }
            
            proveriStripeStatusZaMajstora();
        } else {
            majstorPolja.style.display = 'none';
            firmaDodatno.style.display = 'none';
        }
        
        const mojiTerminiBtn = document.getElementById('mojiTerminiBtn');
        const mojeRezervacijeBtn = document.getElementById('mojeRezervacijeBtn');
        
        if (tipMajstorFirma && mojiTerminiBtn) {
            mojiTerminiBtn.style.display = 'inline-block';
            mojiTerminiBtn.onclick = () => window.location.href = 'moji_termini.html';
        }
        
        if (tipLower === 'klijent' && mojeRezervacijeBtn) {
            mojeRezervacijeBtn.style.display = 'inline-block';
            mojeRezervacijeBtn.onclick = () => window.location.href = 'moje_rezervacije.html';
        }
        
        document.getElementById('editIme').value = korisnik.ime || '';
        document.getElementById('editPrezime').value = korisnik.prezime || '';
        document.getElementById('editTelefon').value = korisnik.telefon || '';
        
        const editRadnoVreme = document.getElementById('editRadnoVreme');
        const editOpis = document.getElementById('editOpis');
        const editNazivFirme = document.getElementById('editNazivFirme');
        const editBrRadnikaUk = document.getElementById('editBrRadnikaUk');
        const editBrRadnikaDost = document.getElementById('editBrRadnikaDost');
        
        if (editRadnoVreme && tipMajstorFirma) {
            const radnoVremePodaci = profil || korisnik;
            const radnoVremeStr = radnoVremePodaci.pocetak_radnog_vremena && radnoVremePodaci.kraj_radnog_vremena 
                ? `${radnoVremePodaci.pocetak_radnog_vremena} - ${radnoVremePodaci.kraj_radnog_vremena}` 
                : '';
            editRadnoVreme.value = radnoVremeStr;
        }
        
        if (editOpis && tipMajstorFirma) {
            editOpis.value = (profil || korisnik).opis_usluga || '';
        }
        
        if (editNazivFirme && tipLower === 'firma') {
            editNazivFirme.value = (profil || korisnik).naziv_firme || '';
        }
        
        if (editBrRadnikaUk && tipLower === 'firma') {
            editBrRadnikaUk.value = (profil || korisnik).broj_ukupnih_radnika || '';
        }
        
        if (editBrRadnikaDost && tipLower === 'firma') {
            editBrRadnikaDost.value = (profil || korisnik).broj_dostupnih_radnika || '';
        }
        
        ucitajObavestenja();
        console.log('Profil uspešno učitan!');
        
    } catch (error) {
        console.error('Greška pri učitavanju profila:', error);
        document.getElementById('imePrezime').innerHTML = 'Greška: ' + error.message;
        document.getElementById('prikazEmail').innerHTML = 'Greška';
        document.getElementById('tipNaloga').innerHTML = 'Greška';
        document.getElementById('prikazTip').innerHTML = 'Greška';
        document.getElementById('prikazTelefon').innerHTML = 'Greška';
        document.getElementById('prikazDatum').innerHTML = 'Greška';
        document.getElementById('prikazRadnoVreme').innerHTML = 'Greška';
    }
}

async function ucitajObavestenja() {
    try {
        const obavestenja = await getObavestenja();
        const container = document.getElementById('listaObavestenja');
        
        if (!obavestenja || obavestenja.length === 0) {
            container.innerHTML = '<div class="text-center tekst-crven small">Nema novih obaveštenja</div>';
            return;
        }
        
        let html = '';
        for (let o of obavestenja) {
            html += `
                <div class="obavestenje-stavka">
                    <i class="fa-solid fa-bell"></i>
                    <span class="obavestenje-naslov">${o.naslov || 'Obaveštenje'}</span>
                </div>
            `;
        }
        container.innerHTML = html;
    } catch (error) {
        console.error('Greška:', error);
        document.getElementById('listaObavestenja').innerHTML = '<div class="text-center tekst-crven small">Greška pri učitavanju</div>';
    }
}


document.getElementById('izmeniPodatkeBtn')?.addEventListener('click', function() {
    document.getElementById('prikazPodataka').style.display = 'none';
    document.getElementById('formaIzmena').style.display = 'block';
    
    const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
    const tipMajstorFirma = (tipLower === 'majstor' || tipLower === 'firma');
    
    if (tipLower === 'majstor' || tipLower === 'firma') {
        document.getElementById('radnoVremePolje').style.display = 'block';
        document.getElementById('opisPolje').style.display = 'block';
    } else {
        document.getElementById('radnoVremePolje').style.display = 'none';
        document.getElementById('opisPolje').style.display = 'none';
    }
    
    if (tipLower === 'firma') {
        document.getElementById('firmaPoljaProfila').style.display = 'block';
    } else {
        document.getElementById('firmaPoljaProfila').style.display = 'none';
    }
    
    const gradoviRadaPolje = document.getElementById('gradoviRadaPolje');
    if (tipMajstorFirma) {
        gradoviRadaPolje.style.display = 'block';
        const container = document.getElementById('gradoviRadaContainerIzmena');
        if (container) {
            ucitajGradoveRadaIzmena('gradoviRadaContainerIzmena');
        }
    } else {
        gradoviRadaPolje.style.display = 'none';
    }
    
    const kategorijePolje = document.getElementById('kategorijePolje');
    if (tipMajstorFirma) {
        kategorijePolje.style.display = 'block';
        ucitajKategorijeZaIzmenu();
    } else {
        kategorijePolje.style.display = 'none';
    }
});

document.getElementById('otkaziIzmenu')?.addEventListener('click', function() {
    document.getElementById('prikazPodataka').style.display = 'block';
    document.getElementById('formaIzmena').style.display = 'none';
    ucitajProfil();
});

document.getElementById('profilForma')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-info';
    alertDiv.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Slanje podataka...';
    document.getElementById('formaIzmena').prepend(alertDiv);
    
    try {
        const data = {
            ime: document.getElementById('editIme').value,
            prezime: document.getElementById('editPrezime').value,
            telefon: document.getElementById('editTelefon').value
        };
        
        if (tipLower === 'klijent') {
            await updateKlijent(data);
        } else if (tipLower === 'majstor') {
            data.opis_usluga = document.getElementById('editOpis').value;
            const radnoVreme = document.getElementById('editRadnoVreme').value.split('-');
            if (radnoVreme.length === 2) {
                data.pocetak_radnog_vremena = radnoVreme[0].trim();
                data.kraj_radnog_vremena = radnoVreme[1].trim();
            }
            data.kategorijeIds = getIzabraneKategorijeIzmena();
            await updateMajstor(data);
            await sacuvajPromeneKategorija();
        } else if (tipLower === 'firma') {
            data.opis_usluga = document.getElementById('editOpis').value;
            const radnoVreme = document.getElementById('editRadnoVreme').value.split('-');
            if (radnoVreme.length === 2) {
                data.pocetak_radnog_vremena = radnoVreme[0].trim();
                data.kraj_radnog_vremena = radnoVreme[1].trim() ;
            }
            data.naziv_firme = document.getElementById('editNazivFirme').value;
            data.broj_ukupnih_radnika = parseInt(document.getElementById('editBrRadnikaUk').value);
            data.broj_dostupnih_radnika = parseInt(document.getElementById('editBrRadnikaDost').value);
            data.kategorijeIds = getIzabraneKategorijeIzmena();
            await updateFirma(data);
            await sacuvajPromeneKategorija();
        }
        
        alertDiv.className = 'alert alert-success';
        alertDiv.innerHTML = '<i class="fa-solid fa-check"></i> Podaci uspešno sačuvani!';
        
        setTimeout(() => {
            document.getElementById('prikazPodataka').style.display = 'block';
            document.getElementById('formaIzmena').style.display = 'none';
            alertDiv.remove();
            ucitajProfil();
        }, 1500);
        
    } catch (error) {
        console.error('Greška:', error);
        alertDiv.className = 'alert alert-danger';
        alertDiv.innerHTML = `<i class="fa-solid fa-triangle-exclamation"></i> ${error.message || 'Greška pri čuvanju podataka'}`;
    }
});


function otvoriModalVerifikacija() {
    const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
    const label = document.getElementById('verifikacijaLabel');
    const poljeLabel = document.getElementById('verifikacijaPoljeLabel');
    const input = document.getElementById('verifikacijaInput');
    const error = document.getElementById('verifikacijaError');
    
    error.style.display = 'none';
    input.value = '';
    
    if (tipLower === 'majstor') {
        label.textContent = 'JMBG';
        poljeLabel.textContent = 'JMBG (13 cifara)';
        input.placeholder = 'Unesite JMBG (13 cifara)';
    } else if (tipLower === 'firma') {
        label.textContent = 'PIB';
        poljeLabel.textContent = 'PIB (9 cifara)';
        input.placeholder = 'Unesite PIB (9 cifara)';
    }
    
    document.getElementById('modalVerifikacija').classList.add('show');
}

function zatvoriModalVerifikacija() {
    document.getElementById('modalVerifikacija').classList.remove('show');
    document.getElementById('verifikacijaError').style.display = 'none';
}

async function posaljiZahtevZaVerifikaciju() {
    const input = document.getElementById('verifikacijaInput');
    const error = document.getElementById('verifikacijaError');
    const tipLower = localStorage.getItem('tip_korisnika')?.toLowerCase();
    const value = input.value.trim();
    
    if (!value) {
        error.textContent = 'Unesite traženi podatak!';
        error.style.display = 'block';
        return;
    }
    
    error.style.display = 'none';
    
    try {
        if (tipLower === 'majstor') {
            if (value.length !== 13) {
                error.textContent = 'JMBG mora imati 13 cifara!';
                error.style.display = 'block';
                return;
            }
            await verifyMajstor(value);
        } 
        else if (tipLower === 'firma') {
            if (value.length !== 9) {
                error.textContent = 'PIB mora imati 9 cifara!';
                error.style.display = 'block';
                return;
            }
            await verifyFirma(value);
        } 
        else {
            error.textContent = 'Samo majstori i firme mogu da se verifikuju!';
            error.style.display = 'block';
            return;
        }
        
        alert('Verifikacija je uspešna!');
        zatvoriModalVerifikacija();
        
        if (trenutniKorisnik) {
            trenutniKorisnik.verifikovan = true;
        }
        
        setTimeout(() => {
            ucitajProfil();
        }, 500);
        
    } catch (error) {
        console.error('Greška:', error);
        const errorDiv = document.getElementById('verifikacijaError');
        if (errorDiv) {
            errorDiv.textContent = error.message || 'Greška pri verifikaciji';
            errorDiv.style.display = 'block';
        }
    }
}


function otvoriModalSliku() {
    document.getElementById('modalSlika').classList.add('show');
    document.getElementById('slikaInput').value = '';
    
    const preview = document.getElementById('slikaPreview');
    const error = document.getElementById('slikaError');
    if (preview) preview.style.display = 'none';
    if (error) error.style.display = 'none';
}

function zatvoriModalSliku() {
    document.getElementById('modalSlika').classList.remove('show');
}

document.getElementById('slikaInput')?.addEventListener('change', function(e) {
    const file = e.target.files[0];
    const preview = document.getElementById('slikaPreview');
    const img = document.getElementById('slikaPreviewImg');
    const error = document.getElementById('slikaError');
    
    error.style.display = 'none';
    
    if (!file) {
        preview.style.display = 'none';
        return;
    }
    
    if (file.size > 2 * 1024 * 1024) {
        error.textContent = 'Slika je prevelika! Maksimalna veličina je 2MB.';
        error.style.display = 'block';
        preview.style.display = 'none';
        return;
    }
    
    const reader = new FileReader();
    reader.onload = function(e) {
        img.src = e.target.result;
        preview.style.display = 'block';
    };
    reader.readAsDataURL(file);
});

async function sacuvajSliku() {
    const fileInput = document.getElementById('slikaInput');
    const errorDiv = document.getElementById('slikaError');
    const file = fileInput.files[0];
    
    if (!file) {
        if (errorDiv) {
            errorDiv.textContent = 'Izaberite sliku!';
            errorDiv.style.display = 'block';
        }
        return;
    }
    
    if (errorDiv) errorDiv.style.display = 'none';
    
    try {
        await posaljiSliku(file);
        alert('Slika je uspešno sačuvana!');
        zatvoriModalSliku();
        ucitajProfil();
    } catch (err) {
        console.error('Greška:', err);
        if (errorDiv) {
            errorDiv.textContent = err.message || 'Greška pri čuvanju slike';
            errorDiv.style.display = 'block';
        }
    }
}


document.getElementById('odjaviSeBtn')?.addEventListener('click', function() {
    if (confirm('Da li ste sigurni da želite da se odjavite?')) {
        odjaviSe();
    }
});

let sveKategorijeIzmena = [];
let izabraneKategorijeIzmenaIds = new Set();

async function ucitajKategorijeZaIzmenu() {
    const container = document.getElementById('kategorijeContainerIzmena');
    const errorDiv = document.getElementById('kategorijeErrorIzmena');
    
    if (errorDiv) errorDiv.style.display = 'none';
    container.innerHTML = '<div class="text-center tekst-crven small p-2">Učitavanje kategorija...</div>';
    
    try {
        sveKategorijeIzmena = await getKategorije();
        console.log('Sve kategorije za izmenu:', sveKategorijeIzmena);
        
        const mojeKategorije = await getIzvodjacKategorije();
        console.log('Moje kategorije:', mojeKategorije);
        
        izabraneKategorijeIzmenaIds = new Set(mojeKategorije.map(k => k.id_kategorije));
        stareKategorijeIds = new Set(izabraneKategorijeIzmenaIds);
        console.log('Izabrani ID-ovi:', izabraneKategorijeIzmenaIds);
        
        prikaziKategorijeHijerarhijaIzmena();
        
    } catch (error) {
        console.error('Greška:', error);
        container.innerHTML = `<div class="text-center tekst-crven small p-2">Greška: ${error.message}</div>`;
    }
}

function prikaziKategorijeHijerarhijaIzmena() {
    const container = document.getElementById('kategorijeContainerIzmena');
    const glavne = sveKategorijeIzmena.filter(k => k.id_roditelja === null);
    
    if (glavne.length === 0) {
        container.innerHTML = '<div class="text-center tekst-crven small p-2">Nema dostupnih kategorija</div>';
        return;
    }
    
    let html = '';
    
    for (let g of glavne) {
        html += `<div class="kategorija-glavna">`;
        html += `<div class="kategorija-glavna-naslov" data-id="${g.id_kategorije}">${g.naziv_kategorije}</div>`;
        
        const podkategorije = sveKategorijeIzmena.filter(k => k.id_roditelja === g.id_kategorije);
        
        if (podkategorije.length > 0) {
            html += `<div class="kategorija-drugi-nivo kategorija-sakriveno" id="nivo2_izmena_${g.id_kategorije}">`;
            
            for (let p of podkategorije) {
                const potomci = sveKategorijeIzmena.filter(k => k.id_roditelja === p.id_kategorije);
                
                if (potomci.length > 0) {
                    html += `<div class="kategorija-drugi-nivo-roditelj">`;
                    html += `<div class="kategorija-drugi-nivo-naslov" data-id="${p.id_kategorije}">${p.naziv_kategorije}</div>`;
                    html += `<div class="kategorija-treci-nivo kategorija-sakriveno" id="nivo3_izmena_${p.id_kategorije}">`;
                    
                    for (let u of potomci) {
                        const checked = izabraneKategorijeIzmenaIds.has(u.id_kategorije) ? 'checked' : '';
                        html += `
                            <label>
                                <input type="checkbox" class="kategorija-checkbox-izmena" 
                                    value="${u.id_kategorije}" ${checked}>
                                ${u.naziv_kategorije}
                            </label>
                        `;
                    }
                    
                    html += `</div></div>`;
                    
                } else {
                    const checked = izabraneKategorijeIzmenaIds.has(p.id_kategorije) ? 'checked' : '';
                    html += `
                        <label>
                            <input type="checkbox" class="kategorija-checkbox-izmena" 
                                value="${p.id_kategorije}" ${checked}>
                            ${p.naziv_kategorije}
                        </label>
                    `;
                }
            }
            
            html += `</div>`;
        }
        
        html += `</div>`;
    }
    
    container.innerHTML = html;
    
    dodajEventListenereKategorijaIzmena();
    azurirajPrikazIzabranihIzmena();
}

function dodajEventListenereKategorijaIzmena() {
    const container = document.getElementById('kategorijeContainerIzmena');
    
    container.querySelectorAll('.kategorija-glavna-naslov').forEach(naslov => {
        naslov.addEventListener('click', function() {
            const id = this.dataset.id;
            const drugiNivo = document.getElementById(`nivo2_izmena_${id}`);
            if (drugiNivo) {
                drugiNivo.classList.toggle('kategorija-sakriveno');
                this.classList.toggle('otvoreno');
            }
        });
    });
    
    container.querySelectorAll('.kategorija-drugi-nivo-naslov').forEach(naslov => {
        naslov.addEventListener('click', function(e) {
            e.stopPropagation();
            const id = this.dataset.id;
            const treciNivo = document.getElementById(`nivo3_izmena_${id}`);
            if (treciNivo) {
                treciNivo.classList.toggle('kategorija-sakriveno');
                this.classList.toggle('otvoreno');
            }
        });
    });
    
    container.querySelectorAll('.kategorija-checkbox-izmena').forEach(cb => {
        cb.addEventListener('change', function() {
            const id = parseInt(this.value);
            if (this.checked) {
                izabraneKategorijeIzmenaIds.add(id);
            } else {
                izabraneKategorijeIzmenaIds.delete(id);
            }
            azurirajPrikazIzabranihIzmena();
        });
    });
}

function azurirajPrikazIzabranihIzmena() {
    const prikazSpan = document.getElementById('prikazIzabranihIzmena');
    
    if (izabraneKategorijeIzmenaIds.size === 0) {
        prikazSpan.innerHTML = 'nijedna';
        return;
    }
    
    const nazivi = [];
    for (let id of izabraneKategorijeIzmenaIds) {
        const kat = sveKategorijeIzmena.find(k => k.id_kategorije === id);
        if (kat) {
            nazivi.push(kat.naziv_kategorije);
        }
    }
    
    prikazSpan.innerHTML = nazivi.map(n => 
        `<span class="izabrana-kategorija-tag">${n}</span>`
    ).join('');
}

function getIzabraneKategorijeIzmena() {
    return Array.from(izabraneKategorijeIzmenaIds);
}

async function sacuvajPromeneKategorija() {
    const noveKategorije = izabraneKategorijeIzmenaIds;
    const stare = stareKategorijeIds;
    
    console.log('Stare kategorije:', stare);
    console.log('Nove kategorije:', noveKategorije);

    const dodate = [];
    for (let id of noveKategorije) {
        if (!stare.has(id)) {
            dodate.push(id);
        }
    }
    
    const uklonjene = [];
    for (let id of stare) {
        if (!noveKategorije.has(id)) {
            uklonjene.push(id);
        }
    }
    
    console.log('Dodate kategorije (ID):', dodate);
    console.log('Uklonjene kategorije (ID):', uklonjene);
    
    for (let id of dodate) {
        try {
            await poveziIzvodjacaIKategoriju(id);
            console.log(`Dodata kategorija ID: ${id}`);
        } catch (error) {
            console.error(`Greška pri dodavanju kategorije ${id}:`, error);
            throw new Error(`Greška pri dodavanju kategorije: ${error.message}`);
        }
    }
    
    for (let id of uklonjene) {
        try {
            await obrisiKategorijuIzvodjaca(id);
            console.log(`Uklonjena kategorija ID: ${id}`);
        } catch (error) {
            console.error(`Greška pri brisanju kategorije ${id}:`, error);
            throw new Error(`Greška pri brisanju kategorije: ${error.message}`);
        }
    }
    
    return { dodate, uklonjene };
}

document.addEventListener('DOMContentLoaded', function() {
    console.log('Profil stranica učitana, pozivam ucitajProfil()');
    ucitajProfil();
});

const urlParams = new URLSearchParams(window.location.search);
const stripeStatus = urlParams.get('stripe');

if (stripeStatus === 'success') {
    alert('Stripe nalog je uspešno povezan!');
    window.history.replaceState({}, document.title, window.location.pathname);
    setTimeout(() => ucitajProfil(), 500);
} else if (stripeStatus === 'error') {
    alert('Došlo je do greške pri povezivanju Stripe naloga.');
    window.history.replaceState({}, document.title, window.location.pathname);
}