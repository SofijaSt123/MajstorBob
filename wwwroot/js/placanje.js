function luhnCheck(brojKartice) {
    let broj = brojKartice.replace(/\s/g, '');
    
    if (broj.length !== 16) return false;
    
    let suma = 0;
    let dupla = false;
    
    for (let i = broj.length - 1; i >= 0; i--) {
        let cifra = parseInt(broj.charAt(i));
        
        if (dupla) {
            cifra *= 2;
            if (cifra > 9) cifra -= 9;
        }
        
        suma += cifra;
        dupla = !dupla;
    }
    
    return (suma % 10) === 0;
}

document.addEventListener('DOMContentLoaded', function() {
    const tipKorisnika = localStorage.getItem('tip_korisnika');
    const korisnikId = localStorage.getItem('korisnik_id');
    
    if (tipKorisnika !== 'klijent') {
        window.location.href = 'index.html';
        return;
    }
    
    const urlParams = new URLSearchParams(window.location.search);
    const zakazivanjeId = urlParams.get('zakazivanje_id');
    const iznos = urlParams.get('iznos');
    
    if (!zakazivanjeId || !iznos) {
        document.getElementById('alertMessage').innerHTML = `
            <div class="alert alert-danger">
                <i class="fa-solid fa-triangle-exclamation"></i> Nedostaju podaci o plaćanju.
            </div>`;
        document.getElementById('paymentForm').style.display = 'none';
        return;
    }
    
    document.getElementById('zakazivanjeId').value = zakazivanjeId;
    document.getElementById('iznos').value = iznos;
    document.getElementById('prikazIznosa').innerHTML = iznos + ' RSD';
    document.getElementById('uslugaInfo').innerHTML = 'Placate uslugu u iznosu od <strong>' + iznos + ' RSD</strong>';
    
    const brojKarticeInput = document.getElementById('brojKartice');
    const datumIstekaInput = document.getElementById('datumIsteka');
    const imeNaKarticiInput = document.getElementById('imeNaKartici');
    
    brojKarticeInput.addEventListener('input', function(e) {
        let value = e.target.value.replace(/\s/g, '');
        if (value.length > 16) value = value.slice(0, 16);
        let formatted = '';
        for (let i = 0; i < value.length; i++) {
            if (i > 0 && i % 4 === 0) formatted += ' ';
            formatted += value[i];
        }
        e.target.value = formatted;
        
        let preview = formatted || '.... .... .... ....';
        document.getElementById('karticaPreview').innerHTML = preview;
    });
    
    datumIstekaInput.addEventListener('input', function(e) {
        let value = e.target.value.replace('/', '');
        if (value.length > 4) value = value.slice(0, 4);
        if (value.length >= 2) {
            value = value.slice(0, 2) + '/' + value.slice(2);
        }
        e.target.value = value;
        document.getElementById('datumPreview').innerHTML = value || 'MM/GG';
    });
    
    imeNaKarticiInput.addEventListener('input', function(e) {
        let value = e.target.value.toUpperCase();
        e.target.value = value;
        document.getElementById('imePreview').innerHTML = value || 'IME PREZIME';
    });
    
    document.getElementById('otkaziPlacanje').addEventListener('click', function() {
        window.location.href = 'moje_rezervacije.html';
    });
    
    document.getElementById('paymentForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const brojKartice = document.getElementById('brojKartice').value.replace(/\s/g, '');
        const datumIsteka = document.getElementById('datumIsteka').value;
        const cvv = document.getElementById('cvv').value;
        const imeNaKartici = document.getElementById('imeNaKartici').value;
        const iznos = document.getElementById('iznos').value;
        const zakazivanjeId = document.getElementById('zakazivanjeId').value;
        
        if (brojKartice.length !== 16) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">Broj kartice mora imati 16 cifara.</div>`;
            return;
        }
        
        if (!datumIsteka.match(/^(0[1-9]|1[0-2])\/([0-9]{2})$/)) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">Datum isteka mora biti u formatu MM/GG.</div>`;
            return;
        }
        
        const [mesec, godina] = datumIsteka.split('/');
        const trenutnaGodina = new Date().getFullYear() % 100;
        const trenutniMesec = new Date().getMonth() + 1;
        
        if (parseInt(godina) < trenutnaGodina || 
            (parseInt(godina) === trenutnaGodina && parseInt(mesec) < trenutniMesec)) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">Datum isteka kartice je prosao.</div>`;
            return;
        }
        
        if (cvv.length < 3 || cvv.length > 4) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">CVV mora imati 3 ili 4 cifre.</div>`;
            return;
        }
        
        if (!imeNaKartici.trim()) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">Unesite ime na kartici.</div>`;
            return;
        }
        
        if (!luhnCheck(brojKartice)) {
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-danger">
                    <i class="fa-solid fa-triangle-exclamation"></i> 
                    Broj kartice nije validan!
                </div>`;
            return;
        }
        
        const submitBtn = document.querySelector('#paymentForm button[type="submit"]');
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Obrada...';
        submitBtn.disabled = true;
        
        setTimeout(() => {
            console.log('Placanje izvrseno:', {
                zakazivanjeId: zakazivanjeId,
                iznos: iznos,
                brojKartice: brojKartice.slice(-4),
                datumIsteka: datumIsteka,
                cvv: '***',
                imeNaKartici: imeNaKartici
            });
            
            const placeneUsluge = JSON.parse(localStorage.getItem('placene_usluge') || '[]');
            placeneUsluge.push({
                zakazivanje_id: parseInt(zakazivanjeId),
                iznos: iznos,
                datum: new Date().toISOString(),
                broj_kartice_poslednje_cetiri: brojKartice.slice(-4)
            });
            localStorage.setItem('placene_usluge', JSON.stringify(placeneUsluge));
            
            document.getElementById('alertMessage').innerHTML = `
                <div class="alert alert-success">
                    <i class="fa-solid fa-circle-check"></i> 
                    Placanje je uspesno izvrseno! Iznos: ${iznos} RSD
                </div>`;
            
            document.getElementById('paymentForm').querySelectorAll('input, button').forEach(el => {
                el.disabled = true;
            });
            
            setTimeout(() => {
                window.location.href = 'moje_rezervacije.html?placeno=1';
            }, 2000);
            
        }, 1500);
    });
});