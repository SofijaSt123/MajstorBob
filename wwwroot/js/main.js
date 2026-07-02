function proveriPristup() {
    const tipKorisnika = localStorage.getItem('tip_korisnika');
    const trenutnaStranica = window.location.pathname;
    const dozvoljeneStranice = ['/pages/login.html', '/pages/register.html'];
    
    if (!tipKorisnika && !dozvoljeneStranice.some(s => trenutnaStranica.includes(s))) {
        window.location.href = 'login.html';
        return false;
    }
    
    if (tipKorisnika) {
        const tipLower = tipKorisnika.toLowerCase();
        
        if (trenutnaStranica.includes('admin.html') && tipLower !== 'admin') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('pretraga.html') && tipLower !== 'klijent') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('moji_termini.html') && tipLower !== 'majstor' && tipLower !== 'firma') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('moje_rezervacije.html') && tipLower !== 'klijent') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('zakazivanje.html') && tipLower !== 'klijent') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('ocene.html') && tipLower !== 'klijent') {
            window.location.href = 'index.html';
            return false;
        }
        if (trenutnaStranica.includes('chat.html') && tipLower === 'admin') {
            window.location.href = 'profil.html';
            return false;
        }
    }
    return true;
}

function preusmeriPremaTipu() {
    const tipKorisnika = localStorage.getItem('tip_korisnika');
    const trenutnaStranica = window.location.pathname;
    
    if (!tipKorisnika) return;

    const tipLower = tipKorisnika.toLowerCase();

    if ((tipLower === 'majstor' || tipLower === 'firma') && trenutnaStranica.includes('index.html')) {
        window.location.href = 'profil.html';
        return;
    }

    if (tipLower === 'admin' && trenutnaStranica.includes('index.html')) {
        window.location.href = 'admin.html';
        return;
    }
}

function generisiMeni() {
    const tipKorisnika = localStorage.getItem('tip_korisnika');
    const container = document.getElementById('meni-container');
    if (!container) return;
    
    const tipLower = tipKorisnika?.toLowerCase();
    let logolink = 'index.html';
    if (tipLower === 'majstor' || tipLower === 'firma') {
        logolink = 'profil.html';
    } else if (tipLower === 'admin') {
        logolink = 'admin.html';
    }
    
    let meniHtml = `
        <nav class="meni navbar navbar-expand-lg">
            <div class="container">
                <a class="meni-logo navbar-brand" href="${logolink}">
                    <i class="fa-solid fa-screwdriver-wrench"></i> MajstorBob
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav ms-auto">
    `;
    
    if (!tipKorisnika) {
        meniHtml += `
            <li class="nav-item"><a class="meni-stavka-link nav-link" href="login.html"><i class="fa-solid fa-arrow-right-from-bracket"></i> Prijava</a></li>
            <li class="nav-item"><a class="meni-stavka-link nav-link" href="register.html"><i class="fa-regular fa-address-card"></i> Registracija</a></li>
        `;
    } 
    else {
        if (tipLower === 'klijent') {
            meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="pretraga.html"><i class="fa-solid fa-magnifying-glass"></i> Pretraga</a></li>`;
        }
        
        if (tipLower === 'majstor' || tipLower === 'firma') {
            meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="moji_termini.html"><i class="fa-solid fa-calendar-check"></i> Moji termini</a></li>`;
        }
        
        if (tipLower === 'klijent' || tipLower === 'majstor' || tipLower === 'firma') {
            meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="chat.html"><i class="fa-solid fa-comments"></i> Chat</a></li>`;
        
        meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="profil.html"><i class="fa-solid fa-user"></i> Moj profil</a></li>`;
        }
        if (tipLower === 'admin') {
            meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="admin.html"><i class="fa-solid fa-user-shield"></i> Admin panel</a></li>`;
        }
    
        meniHtml += `<li class="nav-item"><a class="meni-stavka-link nav-link" href="#" onclick="odjaviSe()"><i class="fa-solid fa-right-from-bracket"></i> Odjava</a></li>`;
    }
    
    meniHtml += `
                    </ul> 
                </div>
            </div>
        </nav>
    `;
    
    container.innerHTML = meniHtml;
}

function odjaviSe() {
    localStorage.removeItem('tip_korisnika');
    localStorage.removeItem('korisnik_id');
    localStorage.removeItem('token');
    localStorage.removeItem('user_ime');
    localStorage.removeItem('user_prezime');
    localStorage.removeItem('user_email');
    localStorage.removeItem('user_telefon');
    localStorage.removeItem('user_cena');
    localStorage.removeItem('user_radno_vreme');
    localStorage.removeItem('klijent_grad');
    localStorage.removeItem('user_profilna_slika');
    localStorage.removeItem('user_datum_registracije');
    
    window.location.href = 'login.html';
}

document.addEventListener('DOMContentLoaded', function() {;
    preusmeriPremaTipu();
    generisiMeni();
    proveriPristup();
});