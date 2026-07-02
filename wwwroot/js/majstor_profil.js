function zvezdice(o) {
    const zaokruzeno = Math.round(o);
    let h = '';
    for (let i = 1; i <= 5; i++) {
        if (i <= zaokruzeno) {
            h += '<i class="fa-solid fa-star zvezdica-puna"></i>';
        } else {
            h += '<i class="far fa-star zvezdica-prazna"></i>';
        }
    }
    return h;
}

function getAvatarUrl(ime, prezime, nazivFirme) {
    if (nazivFirme && !ime) {
        const inicijali = nazivFirme.substring(0, 2).toUpperCase();
        return `https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=120&name=${inicijali}`;
    }
    if (!ime || !prezime) {
        return 'https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=120&name=?';
    }
    const inicijali = ime.charAt(0) + prezime.charAt(0);
    return `https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=120&name=${inicijali}`;
}

async function ucitaj() {
    const url = new URL(window.location.href);
    const izvodjacId = url.searchParams.get('id');
    console.log('ID izvođača:', izvodjacId);
    
    if (!izvodjacId) {
        document.getElementById('majstorImePrezime').innerHTML = 'Izvođač nije pronađen';
        return;
    }

    const majstorIdField = document.getElementById('majstorId');
    if (majstorIdField) majstorIdField.value = izvodjacId;
    
    try {
        let podaci = null;
        let isFirma = false;
        let ime = '';
        let prezime = '';
        let nazivFirme = '';
        let gradoviRada = [];
        
        try {
            podaci = await getMajstorInfoForKlijent(izvodjacId);
            console.log('Podaci o majstoru:', podaci);
            isFirma = false;
            ime = podaci.ime || '';
            prezime = podaci.prezime || '';
        } catch (error) {
            console.log('Nije majstor, pokušavam kao firma...');
            try {
                podaci = await getFirmaInfoForKlijent(izvodjacId);
                console.log('Podaci o firmi:', podaci);
                isFirma = true;
                nazivFirme = podaci.naziv_firme || '';
                ime = podaci.ime || '';
                prezime = podaci.prezime || '';
            } catch (error2) {
                console.error('Greška:', error2);
                document.getElementById('majstorImePrezime').innerHTML = 'Izvođač nije pronađen';
                return;
            }
        }
        
        if (!podaci) {
            document.getElementById('majstorImePrezime').innerHTML = 'Izvođač nije pronađen';
            return;
        }

        try {
            const mojiGradoviRada = await getGradoviRada(izvodjacId);
            console.log('Gradovi rada za izvođača:', mojiGradoviRada);
            
            const sviGradovi = await getGradovi();
            console.log('Svi gradovi:', sviGradovi);
            
            gradoviRada = mojiGradoviRada.map(g => {
                const grad = sviGradovi.find(sg => sg.id_grad === g.id_grada);
                return {
                    id_grad_rada: g.id_grad_rada,
                    id_grada: g.id_grada,
                    naziv_grada: grad ? grad.naziv_grada : 'Nepoznat grad',
                    zona_rada: g.zona_rada || false,
                    doplata: g.doplata || 0
                };
            });
            
            console.log('Gradovi rada sa nazivima:', gradoviRada);
            
        } catch (e) {
            console.warn('Ne mogu da učitam gradove rada:', e);
        }

        const oceneLista = await getOceneMajstora(izvodjacId);
        console.log('Ocene:', oceneLista);
        
        const nazivRed = document.getElementById('firmaNazivRed');
        const nazivSpan = document.getElementById('infoNazivFirme');
        const imePrezimeSpan = document.getElementById('infoImePrezime');
        
        let prikazIme = '';
        let kategorijaTekst = '';
        
        if (isFirma) {
            prikazIme = nazivFirme || `${ime} ${prezime}`.trim() || 'Nepoznata firma';
            kategorijaTekst = 'Firma';
            
            if (nazivRed && nazivSpan) {
                nazivRed.style.display = 'block';
                nazivSpan.innerHTML = nazivFirme || 'Nije uneto';
            }
            
            if (imePrezimeSpan) {
                imePrezimeSpan.innerHTML = `${ime} ${prezime}`.trim() || 'Nepoznato';
            }
            
            document.getElementById('oceniBtn').innerHTML = '<i class="fa-solid fa-star"></i> Ocenite firmu';
            document.getElementById('prijaviBtn').innerHTML = '<i class="fa-solid fa-flag"></i> Prijavi firmu';
            
            const modalNaslov = document.getElementById('modalPrijavaNaslov');
            if (modalNaslov) {
                modalNaslov.innerHTML = '<i class="fa-solid fa-flag"></i> Prijava firme';
            }
            
            const naslov = document.querySelector('.desna-kartica h4');
            if (naslov) {
                naslov.innerHTML = 'O firmi';
            }
            
        } else {
            prikazIme = `${ime} ${prezime}`.trim() || 'Nepoznato';
            kategorijaTekst = 'Majstor';
            
            if (nazivRed) {
                nazivRed.style.display = 'none';
            }
            
            if (imePrezimeSpan) {
                imePrezimeSpan.innerHTML = prikazIme;
            }
            
            document.getElementById('oceniBtn').innerHTML = '<i class="fa-solid fa-star"></i> Ocenite majstora';
            document.getElementById('prijaviBtn').innerHTML = '<i class="fa-solid fa-flag"></i> Prijavi majstora';
            
            const modalNaslov = document.getElementById('modalPrijavaNaslov');
            if (modalNaslov) {
                modalNaslov.innerHTML = '<i class="fa-solid fa-flag"></i> Prijava majstora';
            }
            
            const naslov = document.querySelector('.desna-kartica h4');
            if (naslov) {
                naslov.innerHTML = 'O majstoru';
            }
        }
        
        document.getElementById('majstorImePrezime').innerHTML = prikazIme;
        document.getElementById('majstorKategorija').innerHTML = kategorijaTekst;
        
        const avatarElement = document.getElementById('majstorAvatar');
        const inicijali = (ime ? ime.charAt(0) : '') + (prezime ? prezime.charAt(0) : '');
        avatarElement.src = `https://ui-avatars.com/api/?background=C13B27&color=F5D76E&bold=true&size=150&name=${inicijali || '?'}`;

        if (podaci.profilna_slika) {
            avatarElement.src = `data:image/jpeg;base64,${podaci.profilna_slika}`;
        }
        
        let lokacijaHtml = '';
        if (gradoviRada && gradoviRada.length > 0) {
            const gradoviLista = gradoviRada.map(g => {
                let tekst = g.naziv_grada || 'Nepoznat grad';
                if (g.zona_rada === true && g.doplata > 0) {
                    tekst += ` (doplata: ${g.doplata} EUR)`;
                }
                return tekst;
            });
            lokacijaHtml = gradoviLista.join('<br>');
        } else {
            lokacijaHtml = 'Nije uneto';
        }
        document.getElementById('infoLokacija').innerHTML = lokacijaHtml;
        
        const radnoVremeStr = podaci.pocetak_radnog_vremena && podaci.kraj_radnog_vremena 
            ? `${podaci.pocetak_radnog_vremena} - ${podaci.kraj_radnog_vremena}` 
            : 'Nije uneto';
        document.getElementById('infoRadnoVreme').innerHTML = radnoVremeStr;
        
        document.getElementById('infoOpis').innerHTML = podaci.opis_usluga || 'Nema opisa';
        
        const prosekOcena = podaci.prosek_ocena || 0;
        const brojRecenzija = podaci.broj_recenzija || 0;
        document.getElementById('majstorOcena').innerHTML = zvezdice(prosekOcena) + 
            ' <span class="ms-2">' + prosekOcena.toFixed(1) + '</span>' + 
            ' <span>(broj ocena: ' + brojRecenzija + ')</span>';
        
        const verifikovanBadge = document.getElementById('verifikovanBadgeMajstor');
        if (verifikovanBadge) {
            let jeVerifikovan = podaci.verifikovan || false;
            console.log('Verifikovan status za prikaz:', jeVerifikovan);
            
            if (jeVerifikovan) {
                verifikovanBadge.innerHTML = '<span class="verifikovan-badge"><i class="fa-solid fa-check"></i> Verifikovan</span>';
            } else {
                verifikovanBadge.innerHTML = '<span class="neverifikovan-badge"><i class="fa-solid fa-xmark"></i> Neverifikovan</span>';
            }
        }
        
        let avgCena = 0, avgKvalitet = 0, avgBrzina = 0, avgOdnos = 0;
        if (oceneLista.length > 0) {
            let sumCena = 0, sumKvalitet = 0, sumBrzina = 0, sumOdnos = 0;
            for (let o of oceneLista) {
                sumCena += o.ocena_cena || 0;
                sumKvalitet += o.ocena_kvaliteta || 0;
                sumBrzina += o.ocena_brzine || 0;
                sumOdnos += o.ocena_odnosa || 0;
            }
            avgCena = sumCena / oceneLista.length;
            avgKvalitet = sumKvalitet / oceneLista.length;
            avgBrzina = sumBrzina / oceneLista.length;
            avgOdnos = sumOdnos / oceneLista.length;
        }
        
        document.getElementById('ocenaCena').innerHTML = avgCena > 0 ? avgCena.toFixed(1) : 'Nema ocena';
        document.getElementById('ocenaKvalitet').innerHTML = avgKvalitet > 0 ? avgKvalitet.toFixed(1) : 'Nema ocena';
        document.getElementById('ocenaBrzina').innerHTML = avgBrzina > 0 ? avgBrzina.toFixed(1) : 'Nema ocena';
        document.getElementById('ocenaOdnos').innerHTML = avgOdnos > 0 ? avgOdnos.toFixed(1) : 'Nema ocena';
        
        let recHtml = '';
        if (oceneLista.length > 0) {
            for (let r of oceneLista) {
                const ukupnaOcena = (r.ocena_cena + r.ocena_kvaliteta + r.ocena_brzine + r.ocena_odnosa) / 4;
                recHtml += `
                    <div class="recenzija-stavka">
                        <div class="recenzija-header">
                            <div class="recenzija-zvezdice">${zvezdice(ukupnaOcena)}</div>
                            <span class="recenzija-datum">${r.datum || new Date().toISOString().split('T')[0]}</span>
                        </div>
                        <p class="recenzija-tekst">${r.opis_recenzije || 'Nema komentara'}</p>
                        ${r.odgovor ? `<div class="recenzija-odgovor"><strong>Odgovor izvođača:</strong> ${r.odgovor}</div>` : ''}
                    </div>
                `;
            }
        } else {
            recHtml = '<div class="recenzija-prazno">Nema recenzija</div>';
        }
        document.getElementById('recenzijeLista').innerHTML = recHtml;
        
        document.getElementById('zakaziBtn').href = 'zakazivanje.html?majstor_id=' + izvodjacId;
        document.getElementById('oceniBtn').href = 'ocene.html?majstor_id=' + izvodjacId;
        
        if (isFirma) {
            const dodatniInfo = document.createElement('div');
            dodatniInfo.className = 'info-red';
            dodatniInfo.id = 'firmaDodatniInfo';
            dodatniInfo.innerHTML = `
                <span class="oznaka-polja">Broj radnika:</span>
                <span class="tekst-crven">${podaci.broj_ukupnih_radnika || 0} (dostupno: ${podaci.broj_dostupnih_radnika || 0})</span>
            `;
            
            const radnoVremeRed = document.querySelector('.info-red:has(#infoRadnoVreme)');
            if (radnoVremeRed && radnoVremeRed.parentNode) {
                const postoji = document.getElementById('firmaDodatniInfo');
                if (!postoji) {
                    radnoVremeRed.parentNode.insertBefore(dodatniInfo, radnoVremeRed.nextSibling);
                }
            }
        } else {
            const firmaInfo = document.getElementById('firmaDodatniInfo');
            if (firmaInfo) {
                firmaInfo.remove();
            }
        }
        
    } catch (error) {
        console.error('Greška pri učitavanju:', error);
        document.getElementById('majstorImePrezime').innerHTML = 'Greška pri učitavanju podataka';
    }
}

function otvoriModalPrijavu() {
    const imePrezime = document.getElementById('majstorImePrezime').innerText;
    const kategorija = document.getElementById('majstorKategorija').innerText;
    
    document.getElementById('prijavaMajstorIme').innerHTML = `<strong>${imePrezime}</strong>`;
    
    const modalNaslov = document.getElementById('modalPrijavaNaslov');
    if (kategorija.toLowerCase() === 'firma') {
        modalNaslov.innerHTML = '<i class="fa-solid fa-flag"></i> Prijava firme';
    } else {
        modalNaslov.innerHTML = '<i class="fa-solid fa-flag"></i> Prijava majstora';
    }
    
    document.getElementById('modalPrijava').classList.add('show');
}

function zatvoriModalPrijavu() {
    document.getElementById('modalPrijava').classList.remove('show');
    document.getElementById('prijavaRazlog').value = '';
}

async function posaljiPrijavu() {
    const razlog = document.getElementById('prijavaRazlog').value.trim();
    if (!razlog) {
        alert('Morate uneti razlog prijave!');
        return;
    }
    
    const id = new URLSearchParams(window.location.search).get('id');
    console.log('Šaljem prijavu za ID:', id);
    console.log('Razlog:', razlog);
    
    try {
        await kreirajPrijavu({
            id_izvodjaca: parseInt(id),
            razlog: razlog
        });
        alert('Prijava je poslata! Administrator će je razmotriti.');
        zatvoriModalPrijavu();
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri slanju prijave. Pokušajte ponovo.');
    }
}

document.getElementById('zapocniChatBtn')?.addEventListener('click', async function() {
    const url = new URL(window.location.href);
    const izvodjacId = url.searchParams.get('id');
    
    if (!izvodjacId) {
        alert('ID majstora nije pronađen!');
        return;
    }
    
    const token = localStorage.getItem('token');
    if (!token) {
        alert('Morate biti prijavljeni da biste započeli razgovor!');
        window.location.href = 'login.html';
        return;
    }
    
    const tipKorisnika = localStorage.getItem('tip_korisnika');
    if (tipKorisnika?.toLowerCase() !== 'klijent') {
        alert('Samo klijenti mogu da započinju razgovore sa majstorima!');
        return;
    }
    
    try {
        const btn = document.getElementById('zapocniChatBtn');
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Kreiranje...';
        btn.disabled = true;
        
        await kreirajRazgovor(parseInt(izvodjacId));
        
        await new Promise(resolve => setTimeout(resolve, 500));
        
        const razgovori = await getRazgovore();
        console.log('Svi razgovori:', razgovori);
        
        const pronadjen = razgovori.find(r => r.sagovornik_id == izvodjacId);
        
        if (pronadjen) {
            window.location.href = `chat.html?razgovor_id=${pronadjen.id_razgovora}`;
        } else {
            window.location.href = 'chat.html';
        }
        
    } catch (error) {
        console.error('Greška:', error);
        alert('Greška pri kreiranju razgovora. Pokušajte ponovo.');
        
        const btn = document.getElementById('zapocniChatBtn');
        btn.innerHTML = '<i class="fa-solid fa-comment"></i> Započni razgovor';
        btn.disabled = false;
    }
});

const tipKorisnika = localStorage.getItem('tip_korisnika'); 
const mojiTerminiBtn = document.getElementById('mojiTerminiBtn');
const tipLower = tipKorisnika?.toLowerCase();

if (mojiTerminiBtn && (tipLower === 'majstor' || tipLower === 'firma')) {
    mojiTerminiBtn.style.display = 'inline-block';
    mojiTerminiBtn.addEventListener('click', function() {
        window.location.href = 'moji_termini.html';
    });
}

window.onload = ucitaj;