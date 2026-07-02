let ocene = { cena: 0, kvalitet: 0, brzina: 0, odnos: 0 };
let majstorId = null;

function napraviZvezdice(field) {
    const container = document.querySelector(`.zvezdice[data-field="${field}"]`);
    if (!container) return;
    
    container.innerHTML = '';
    
    for (let i = 1; i <= 5; i++) {
        const star = document.createElement('i');
        star.className = 'fa-regular fa-star';
        star.style.fontSize = '60px';
        star.dataset.value = i;
        star.addEventListener('click', () => {
            ocene[field] = i;
            osveziZvezdice(field);
        });
        star.addEventListener('mouseenter', () => {
            for (let j = 1; j <= 5; j++) {
                const s = container.children[j-1];
                if (j <= i) s.classList.add('hover');
                else s.classList.remove('hover');
            }
        });
        container.appendChild(star);
    }
    container.addEventListener('mouseleave', () => osveziZvezdice(field));
}

function osveziZvezdice(field) {
    const container = document.querySelector(`.zvezdice[data-field="${field}"]`);
    if (!container) return;
    for (let i = 1; i <= 5; i++) {
        const star = container.children[i-1];
        if (i <= ocene[field]) {
            star.className = 'fa-solid fa-star selected';
        } else {
            star.className = 'fa-regular fa-star';
        }
        star.classList.remove('hover');
    }
}

async function ucitaj() {
    const urlParams = new URLSearchParams(window.location.search);
    majstorId = urlParams.get('majstor_id');
    
    if (!majstorId) {
        document.getElementById('majstorInfo').innerHTML = 'Izvođač nije pronađen';
        return;
    }
    
    document.getElementById('majstorId').value = majstorId;
    
    try {
        let ime = '';
        let prezime = '';
        let nazivFirme = '';
        let isFirma = false;
        
        try {
            const podaci = await getMajstorInfoForKlijent(majstorId);
            ime = podaci.ime || '';
            prezime = podaci.prezime || '';
        } catch (e) {
            try {
                const podaci = await getFirmaInfoForKlijent(majstorId);
                isFirma = true;
                nazivFirme = podaci.naziv_firme || '';
                ime = podaci.ime || '';
                prezime = podaci.prezime || '';
            } catch (e2) {
                console.warn('Ne mogu da učitam podatke:', e2);
            }
        }
        
        // ===== PROMENI NASLOV U ZAVISNOSTI DA LI JE FIRMA ILI MAJSTOR =====
        const naslov = document.getElementById('ocenaNaslov');
        const info = document.getElementById('majstorInfo');
        
        if (isFirma) {
            naslov.textContent = 'Ocenite firmu';
            info.innerHTML = `Ocenjujete firmu <strong>${nazivFirme}</strong>`;
        } else {
            naslov.textContent = 'Ocenite majstora';
            info.innerHTML = `Ocenjujete majstora <strong>${ime} ${prezime}</strong>`;
        }
        
    } catch (error) {
        console.error('Greška:', error);
        document.getElementById('majstorInfo').innerHTML = 'Ocenjivanje majstora';
    }
    
    napraviZvezdice('cena');
    napraviZvezdice('kvalitet');
    napraviZvezdice('brzina');
    napraviZvezdice('odnos');
}

document.getElementById('odustaniBtn')?.addEventListener('click', function() {
    if (majstorId) {
        window.location.href = `majstor_profil.html?id=${majstorId}`;
    } else {
        window.location.href = 'pretraga.html';
    }
});

document.getElementById('ocenaForma')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const alertDiv = document.getElementById('alertMessage');
    
    if (ocene.cena === 0 || ocene.kvalitet === 0 || ocene.brzina === 0 || ocene.odnos === 0) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Ocenite sve kategorije!</div>';
        return;
    }
    
    const recenzija = document.getElementById('recenzija').value.trim();
    if (!recenzija) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Napišite recenziju!</div>';
        return;
    }
    
    const token = localStorage.getItem('token');
    if (!token) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Morate biti prijavljeni da biste ocenili majstora!</div>';
        setTimeout(() => window.location.href = 'login.html', 1500);
        return;
    }
    
    if (!majstorId) {
        alertDiv.innerHTML = '<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> Majstor nije odabran!</div>';
        return;
    }
    
    const ocenaData = {
        ocena_cena: ocene.cena,
        ocena_kvaliteta: ocene.kvalitet,
        ocena_brzine: ocene.brzina,
        ocena_odnosa: ocene.odnos,
        opis_recenzije: recenzija
    };
    
    console.log('Šaljem ocenu:', ocenaData);
    
    try {
        alertDiv.innerHTML = '<div class="alert alert-info"><i class="fa-solid fa-spinner fa-spin"></i> Slanje ocene...</div>';
        
        await posaljiOcenu(majstorId, ocenaData);
        
        alertDiv.innerHTML = '<div class="alert alert-success"><i class="fa-solid fa-check-circle"></i> Ocena poslata! Hvala. Preusmeravamo...</div>';
        setTimeout(() => {
            window.location.href = `majstor_profil.html?id=${majstorId}`;
        }, 1500);
        
    } catch (error) {
        console.error('Greška:', error);
        alertDiv.innerHTML = `<div class="alert alert-danger"><i class="fa-solid fa-triangle-exclamation"></i> ${error.message || 'Došlo je do greške. Pokušajte ponovo.'}</div>`;
    }
});

window.onload = ucitaj;