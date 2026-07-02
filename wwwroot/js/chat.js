let trenutniRazgovorId = null;
let pollingInterval = null;
let sviRazgovori = [];
let samoPoslataPoruka = false;  

const tipKorisnika = localStorage.getItem('tip_korisnika');
const korisnikId = localStorage.getItem('korisnik_id');

if (!tipKorisnika || !korisnikId) {
    window.location.href = 'login.html';
}

if (tipKorisnika?.toLowerCase() === 'admin') {
    window.location.href = 'profil.html';
}

function formatirajVreme(porukaVreme) {
    const datum = new Date(porukaVreme);
    const sada = new Date();
    const danas = new Date(sada.getFullYear(), sada.getMonth(), sada.getDate());
    const juce = new Date(danas);
    juce.setDate(juce.getDate() - 1);
    
    const vremeStr = datum.toLocaleTimeString('sr-RS', { hour: '2-digit', minute: '2-digit' });
    
    if (datum >= danas) {
        return vremeStr;
    }
    
    if (datum >= juce && datum < danas) {
        return `Juče ${vremeStr}`;
    }
    
    const pre7Dana = new Date(danas);
    pre7Dana.setDate(pre7Dana.getDate() - 6);
    if (datum >= pre7Dana) {
        const dani = ['Nedelja', 'Ponedeljak', 'Utorak', 'Sreda', 'Četvrtak', 'Petak', 'Subota'];
        const imeDana = dani[datum.getDay()];
        return `${imeDana} ${vremeStr}`;
    }
    
    const datumStr = datum.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' });
    return `${datumStr} ${vremeStr}`;
}

document.addEventListener('DOMContentLoaded', async function() {
    await ucitajRazgovore();
    
    document.getElementById('pretragaChat').addEventListener('input', filtrirajRazgovore);
    document.getElementById('novaPoruka').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') posaljiPoruku();
    });
    document.getElementById('posaljiPorukuBtn').addEventListener('click', posaljiPoruku);
});


async function ucitajRazgovore() {
    try {
        const razgovori = await getRazgovore();
        console.log('Razgovori iz API-ja:', razgovori);
        
        sviRazgovori = razgovori.map(r => ({
            id: r.id_razgovora,
            ime_sagovornika: r.name || 'Nepoznat',
            sagovornik_id: r.sagovornik_id || 0,
            tip_sagovornika: r.tip_sagovornika || 'Korisnik'
        }));
        prikaziListuRazgovora(sviRazgovori);
    } catch (error) {
        console.error('Greška:', error);
        sviRazgovori = [];
        prikaziListuRazgovora(sviRazgovori);
    }
}

function prikaziListuRazgovora(razgovori) {
    const container = document.getElementById('listaRazgovora');
    
    if (!razgovori || razgovori.length === 0) {
        container.innerHTML = `<div class="p-4 text-center tekst-crven">Nemate aktivnih razgovora</div>`;
        return;
    }
    
    let html = '';
    for (let i = 0; i < razgovori.length; i++) {
        const r = razgovori[i];
        const aktivnaKlasa = (trenutniRazgovorId === r.id) ? 'aktivan' : '';
        
        html += `
            <div class="chat-stavka ${aktivnaKlasa}" 
                data-id="${r.id}" 
                data-ime="${r.ime_sagovornika}" 
                data-sagovornik="${r.sagovornik_id}"
                data-tip="${r.tip_sagovornika}">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="chat-ime">
                        <i class="fa-solid fa-circle-user"></i> ${r.ime_sagovornika}
                    </div>
                </div>
            </div>
        `;
    }
    container.innerHTML = html;
    
    document.querySelectorAll('.chat-stavka').forEach(stavka => {
        stavka.addEventListener('click', function(e) {
            const id = parseInt(this.dataset.id);
            const ime = this.dataset.ime;
            const sagovornikId = parseInt(this.dataset.sagovornik);
            const tip = this.dataset.tip;
            otvoriRazgovor(id, ime, sagovornikId, tip);
        });
    });
}

function filtrirajRazgovore() {
    const pojam = document.getElementById('pretragaChat').value.toLowerCase();
    if (!pojam) {
        prikaziListuRazgovora(sviRazgovori);
        return;
    }
    
    const filtrirani = sviRazgovori.filter(r => 
        r.ime_sagovornika.toLowerCase().includes(pojam)
    );
    prikaziListuRazgovora(filtrirani);
}

async function otvoriRazgovor(razgovorId, imeSagovornika, sagovornikId, tipSagovornika) {
    if (trenutniRazgovorId === razgovorId) return;
    
    trenutniRazgovorId = razgovorId;
    
    document.querySelectorAll('.chat-stavka').forEach(el => el.classList.remove('aktivan'));
    document.querySelector(`.chat-stavka[data-id="${razgovorId}"]`)?.classList.add('aktivan');
    
    document.getElementById('chatImeSagovornika').innerHTML = imeSagovornika;
    document.getElementById('chatInputArea').style.display = 'flex';
    
    await ucitajPoruke(razgovorId, imeSagovornika);
    
    if (pollingInterval) clearInterval(pollingInterval);
    pollingInterval = setInterval(() => {
        if (trenutniRazgovorId) {
            ucitajPoruke(trenutniRazgovorId, imeSagovornika, true);
        }
    }, 3000);
}

async function ucitajPoruke(razgovorId, imeSagovornika, silent = false) {
    if (samoPoslataPoruka) return;
    
    try {
        const poruke = await getPoruke(razgovorId);
        prikaziPoruke(poruke, imeSagovornika);
    } catch (error) {
        if (!silent) {
            console.error('Greška:', error);
            const container = document.getElementById('chatPoruke');
            container.innerHTML = `<div class="prazno-stanje"><p>Greška pri učitavanju poruka. Pokušajte ponovo.</p></div>`;
        }
    }
}

function prikaziPoruke(poruke, imeSagovornika) {
    const container = document.getElementById('chatPoruke');
    const mojId = parseInt(korisnikId);
    
    if (!poruke || poruke.length === 0) {
        container.innerHTML = `<div class="prazno-stanje"><p>Nema poruka. Započnite razgovor sa ${imeSagovornika}!</p></div>`;
        return;
    }
    
    let html = '';
    for (const poruka of poruke) {
        const jeMoja = (poruka.posiljac_id === mojId);
        const vreme = formatirajVreme(poruka.vreme);
        const tekst = poruka.tekst.replace(/</g, '&lt;').replace(/>/g, '&gt;');
        
        html += `
            <div class="poruka ${jeMoja ? 'poruka-moja' : 'poruka-drugi'}">
                <div class="poruka-bubble">${tekst}</div>
                <div class="poruka-vreme">${vreme}</div>
            </div>
        `;
    }
    
    container.innerHTML = html;
    container.scrollTop = container.scrollHeight;
}

async function posaljiPoruku() {
    const tekst = document.getElementById('novaPoruka').value.trim();
    if (!tekst || !trenutniRazgovorId) {
        if (!tekst) alert('Unesite poruku!');
        return;
    }

    const btn = document.getElementById('posaljiPorukuBtn');
    const originalHtml = btn.innerHTML;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
    btn.disabled = true;

    try {
        await posaljiPorukuApi({
            razgovorId: trenutniRazgovorId,
            tekst: tekst
        });

        const container = document.getElementById('chatPoruke');
        const sada = new Date();
        const vreme = formatirajVreme(sada.toISOString());
        const zaštitaTeksta = tekst.replace(/</g, '&lt;').replace(/>/g, '&gt;');

        const novaPorukaHtml = `
            <div class="poruka poruka-moja">
                <div class="poruka-bubble">${zaštitaTeksta}</div>
                <div class="poruka-vreme">${vreme}</div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', novaPorukaHtml);
        container.scrollTop = container.scrollHeight;
        document.getElementById('novaPoruka').value = '';

        samoPoslataPoruka = true;
        setTimeout(() => {
            samoPoslataPoruka = false;
        }, 2000);

    } catch (error) {
        console.error('Greška pri slanju poruke:', error);
        alert('Greška pri slanju poruke. Pokušajte ponovo.');
    } finally {
        btn.innerHTML = originalHtml;
        btn.disabled = false;
        document.getElementById('novaPoruka').focus();
    }
}