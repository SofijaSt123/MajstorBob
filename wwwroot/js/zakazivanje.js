async function ucitaj() {
    const urlParams = new URLSearchParams(window.location.search);
    const izvodjacId = urlParams.get('majstor_id');
    
    console.log('ID iz URL-a:', izvodjacId);
    
    if (!izvodjacId) {
        document.getElementById('majstorInfo').innerHTML = 'Izvođač nije pronađen';
        return;
    }
    
    document.getElementById('majstorId').value = izvodjacId;
    
    try {
        let ime = '';
        let prezime = '';
        let nazivFirme = '';
        let isFirma = false;
        let podaci = null;
        
        try {
            console.log('Pozivam getFirmaInfoForKlijent za ID:', izvodjacId);
            podaci = await getFirmaInfoForKlijent(izvodjacId);
            console.log('Firma podaci:', podaci);
            isFirma = true;
            nazivFirme = podaci.naziv_firme || '';
            ime = podaci.ime || '';
            prezime = podaci.prezime || '';
        } catch (error) {
            console.warn('Nije firma ili greška:', error.message);
            try {
                console.log('Pozivam getMajstorInfoForKlijent za ID:', izvodjacId);
                podaci = await getMajstorInfoForKlijent(izvodjacId);
                console.log('Majstor podaci:', podaci);
                isFirma = false;
                ime = podaci.ime || '';
                prezime = podaci.prezime || '';
            } catch (error2) {
                console.error('Greška pri učitavanju podataka:', error2.message);
                document.getElementById('majstorInfo').innerHTML = 'Izvođač nije pronađen';
                return;
            }
        }
        
        if (isFirma) {
            document.getElementById('majstorInfo').innerHTML = `Zakazujete kod firme <strong>${nazivFirme}</strong>`;
        } else {
            const imePrezime = `${ime} ${prezime}`.trim() || 'Majstor';
            document.getElementById('majstorInfo').innerHTML = `Zakazujete kod majstora <strong>${imePrezime}</strong>`;
        }
        
    } catch (error) {
        console.error('Greška:', error);
        document.getElementById('majstorInfo').innerHTML = 'Izvođač nije pronađen';
    }
}

document.getElementById('odustaniBtn')?.addEventListener('click', function() {
    const id = document.getElementById('majstorId').value;
    if (id) {
        window.location.href = `majstor_profil.html?id=${id}`;
    } else {
        window.location.href = 'pretraga.html';
    }
});

document.getElementById('zakazivanjeForma')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const majstorId = document.getElementById('majstorId').value;
    const datum = document.getElementById('datum').value;
    let vreme = document.getElementById('vreme').value;
    const adresa = document.getElementById('adresa').value;
    const opis = document.getElementById('opisRadova').value;
    const alertDiv = document.getElementById('alertMessage');

    if (vreme && !vreme.includes(':')) {
        vreme = vreme + ':00:00';
    } else if (vreme && vreme.split(':').length === 2) {
        vreme = vreme + ':00';
    }
    
    if (!datum || !vreme || !adresa || !opis) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Popunite sva polja!</div>';
        return;
    }
    
    if (!majstorId) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Majstor nije odabran!</div>';
        return;
    }
    
    const token = localStorage.getItem('token');
    if (!token) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Morate biti prijavljeni da zakažete uslugu!</div>';
        setTimeout(() => window.location.href = 'login.html', 1500);
        return;
    }
    
    const zahtevData = {
        id_izvodjaca: parseInt(majstorId),
        datum: datum,
        vreme: vreme,
        opis_radova: opis,
        adresa: adresa
    };
    
    console.log('Šaljem zahtev:', zahtevData);
    
    try {
        alertDiv.innerHTML = '<div class="alert alert-info"><i class="fa-solid fa-spinner fa-spin"></i> Slanje zahteva...</div>';
        
        await posaljiZahtev(zahtevData);
        
        alertDiv.innerHTML = '<div class="alert alert-success"><i class="fa-solid fa-check-circle"></i> Zahtev je poslat! Majstor će vam odgovoriti uskoro. Preusmeravamo...</div>';
        setTimeout(() => {
            window.location.href = 'pretraga.html';
        }, 2000);
        
    } catch (error) {
        console.error('Greška:', error);
        alertDiv.innerHTML = `<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> ${error.message || 'Došlo je do greške. Pokušajte ponovo.'}</div>`;
    }
});

window.onload = ucitaj;