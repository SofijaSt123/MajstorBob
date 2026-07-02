const API_BASE = 'http://localhost:5137/api';

function parseJwt(token) {
    try {
        if (!token || typeof token !== 'string') {
            console.error('Token nije string:', token);
            return null;
        }
        
        const parts = token.split('.');
        if (parts.length !== 3) {
            console.error('Token nema 3 dela (nije JWT):', parts.length);
            return null;
        }
        
        const base64Url = parts[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error('Greška pri dekodiranju tokena:', e);
        return null;
    }
}

function getToken() {
    return localStorage.getItem('token');
}

function getHeaders() {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
}

async function register(userData) {
    const tipMap = {
        'klijent': 0,
        'majstor': 1,
        'firma': 2,
        'admin': 3
    };

    const payload = {
        email: userData.email,
        lozinka: userData.lozinka,
        ime: userData.ime,
        prezime: userData.prezime,
        tip_korisnika: tipMap[userData.tip_korisnika],
        telefon: userData.telefon || "",
        kategorijeIds: userData.kategorije || [],
        naziv: userData.naziv_firme || "",          
        pocetakRV: userData.pocetak_radnog_vremena || null,  
        krajRV: userData.kraj_radnog_vremena || null,        
        brUkupni: userData.broj_ukupnih_radnika || 0,        
        brDostupnih: userData.broj_dostupnih_radnika || 0,
    };

    console.log('Slanje:', payload);

    const response = await fetch(`${API_BASE}/Korisnici/Create`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    const token = await response.text();

    if (!response.ok) {
        throw new Error(`Server error: ${response.status} - ${token}`);
    }

    console.log('Token:', token);
    
    return { success: true, message: 'Uspešno ste registrovani! Molimo prijavite se.' };
}

async function login(email, password) {
    try {
        const response = await fetch(`${API_BASE}/Korisnici/LoginUser?Email=${encodeURIComponent(email)}&lozinka=${encodeURIComponent(password)}`, {
            method: 'POST'
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('Login greška:', errorText);
            return { success: false, message: 'Blokirani ste! Ne možete se prijaviti na portal.' };
        }

        const token = await response.text();
        console.log('Token odgovor:', token);

        if (!token || token === 'false' || token === '' || token.includes('System.') || token.includes('Exception')) {
            console.error('Nevalidan token:', token);
            return { success: false, message: 'Pogrešan email ili lozinka' };
        }

        localStorage.setItem('token', token);
        
        const decoded = parseJwt(token);
        console.log('Decoded token:', decoded);
        
        if (!decoded) {
            console.error('Token nije validan JWT');
            localStorage.removeItem('token');
            return { success: false, message: 'Greška pri prijavi. Pokušajte ponovo.' };
        }
        
        const korisnikId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'] || 
                            decoded['sid'] || 
                            decoded['nameid'] ||
                            decoded['id'];
        
        const tipKorisnika = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
                            decoded['role'] ||
                            decoded['tip_korisnika'];
        
        if (!korisnikId || !tipKorisnika) {
            console.error('Nedostaju podaci u tokenu:', { korisnikId, tipKorisnika });
            localStorage.removeItem('token');
            return { success: false, message: 'Greška pri prijavi. Pokušajte ponovo.' };
        }
        
        localStorage.setItem('korisnik_id', korisnikId);
        
        const tipLower = tipKorisnika?.toLowerCase();
        let redirectUrl = 'index.html';
        if (tipLower === 'admin') {
            redirectUrl = 'admin.html';
        } else if (tipLower === 'majstor' || tipLower === 'firma') { 
            redirectUrl = 'profil.html';
        }

        return { 
            success: true, 
            token: token, 
            message: 'Uspešno ste prijavljeni!',
            data: { 
                tip_korisnika: tipKorisnika, 
                id: korisnikId,
                redirectUrl: redirectUrl 
            } 
        };
        
    } catch (error) {
        console.error('Login error:', error);
        return { success: false, message: 'Došlo je do greške. Proverite konekciju.' };
    }
}

async function getKlijent() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Klijenti/KlijentInfo`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam podatke klijenta');
    }
    const text = await response.text();
    if (!text) {
        return null;
    }
    return JSON.parse(text);
}

async function getMajstor() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Majstor/MajstorInfo`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam podatke majstora');
    }
    const text = await response.text();
    if (!text) {
        return null;
    }
    return JSON.parse(text);
}

async function getFirma() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Firme/FirmaInfo`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam podatke firme');
    }
    const text = await response.text();
    if (!text) {
        return null;
    }
    return JSON.parse(text);
}

async function getKategorije() {
    const response = await fetch(`${API_BASE}/Kategorije/Vrati sve kategorije`);
    if (!response.ok) {
        throw new Error('Ne mogu da učitam kategorije');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function getIzvodjacKategorije() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Kategorije/GetIzvodjacKategorije`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam kategorije izvođača');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function poveziIzvodjacaIKategoriju(idKategorije) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/../povezi izvodja i kategoriju?id_kategorije=${idKategorije}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri povezivanju kategorije');
    }
    
    return await response.text();
}

async function obrisiKategorijuIzvodjaca(idKategorije) {
    const token = getToken();
    const response = await fetch(`http://localhost:5137/ObrisiKategorijuIzvodjaca?id_kategorije=${idKategorije}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri brisanju kategorije');
    }
    
    return await response.text();
}

async function getGradovi() {
    const response = await fetch(`${API_BASE}/Gradovi/VratiGradove`);
    if (!response.ok) {
        throw new Error('Ne mogu da učitam gradove');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function pretraziIzvodjace(kategorijaId, gradId, minOcena, doplata) {
    const token = getToken();
    let url = `${API_BASE}/GradoviRada/GetIzvodjace?`;
    
    const params = [];
    if (kategorijaId) params.push(`id_kategorije=${kategorijaId}`);
    if (gradId) params.push(`id_grad=${gradId}`);
    if (minOcena > 0) params.push(`ocena=${minOcena}`);
    if (doplata !== null && doplata !== undefined) {
        params.push(`doplata=${doplata}`);
    }
    
    url += params.join('&');
    
    console.log('Pretraga URL:', url);
    
    const response = await fetch(url, {
        headers: getHeaders()
    });
    
    if (!response.ok) {
        throw new Error('Greška pri pretrazi majstora');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function getMajstorInfoForKlijent(id) {
    try {
        const response = await fetch(`${API_BASE}/Majstor/MajstorInfoForKlijent/${id}`);
        if (!response.ok) {
            if (response.status === 404 || response.status === 500) {
                console.warn(`Majstor sa ID ${id} nije pronađen (verovatno je firma)`);
                return null; 
            }
            throw new Error('Ne mogu da učitam podatke majstora');
        }
        const text = await response.text();
        if (!text) {
            return null;
        }
        return JSON.parse(text);
    } catch (error) {
        console.warn(`Greška pri dohvatanju majstora ${id}:`, error.message);
        return null;
    }
}

async function getFirmaInfoForKlijent(id) {
    try {
        const response = await fetch(`${API_BASE}/Firme/FirmaInfoForKlijent/${id}`);
        if (!response.ok) {
            if (response.status === 404 || response.status === 500) {
                console.warn(`Firma sa ID ${id} nije pronađena`);
                return null;  
            }
            throw new Error('Ne mogu da učitam podatke firme');
        }
        const text = await response.text();
        if (!text) {
            return null;
        }
        return JSON.parse(text);
    } catch (error) {
        console.warn(`Greška pri dohvatanju firme ${id}:`, error.message);
        return null;
    }
}


async function getOceneMajstora(izvodjacId) {
    const response = await fetch(`${API_BASE}/Ocena/Vratiocenemajstora?idIzvodjaca=${izvodjacId}`);
    if (!response.ok) {
        throw new Error('Ne mogu da učitam ocene');
    }
    const text = await response.text();
    if (!text || text === "Nema ocena") {
        return [];
    }
    return JSON.parse(text);
}

async function posaljiOcenu(majstorId, ocenaData) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Ocena/Ostavirecenziju?idIzvodjaca=${majstorId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(ocenaData)
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri slanju ocene: ${error}`);
    }
    return await response.text();
}

async function verifyMajstor(jmbg) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Majstor/VerifyMajstor`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: `"${jmbg}"`
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri verifikaciji majstora: ${error}`);
    }
    return await response.text();
}

async function verifyFirma(pib) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Firme/VerifyFirma`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: `"${pib}"`
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri verifikaciji firme: ${error}`);
    }
    return await response.text();
}

async function posaljiSliku(file) {
    const token = getToken();
    const formData = new FormData();
    formData.append('slika', file);
    
    const response = await fetch(`${API_BASE}/Korisnici/SetSlika`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`
        },
        body: formData
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri uploadu slike: ${error}`);
    }
    return await response.text();
}

async function getSlika() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Korisnici/GetSlika`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam sliku');
    }
    const data = await response.json();
    return data.image;  
}

async function getGradoviRada(idIzvodjaca) {
    const token = getToken();
    let url = `${API_BASE}/GradoviRada/GetGradRada`;
    if (idIzvodjaca) {
        url += `?id_izvodjaca=${idIzvodjaca}`;
    }
    const response = await fetch(url, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam gradove rada');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function dodajGradRada(gradData) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/GradoviRada/dodajGradGada`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id_grada: gradData.id_grada,
            zona_rada: gradData.zona_rada,
            doplata: gradData.doplata || 0
        })
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri dodavanju grada: ${error}`);
    }
    return await response.text();
}

async function deleteGradRada(id) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/GradoviRada/DeleteGradRada/${id}`, {
        method: 'DELETE',
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da obrišem grad rada');
    }
    return await response.text();
}

async function posaljiObavestenje(data) {
    const token = getToken();
    
    const payload = {
        naslov: data.naslov
    };
    
    if (data.receiver_id) {
        payload.receiver_id = data.receiver_id;
        payload.tip_kome_saljes = null;
    } else if (data.tip_kome_saljes !== null && data.tip_kome_saljes !== undefined) {
        payload.receiver_id = null;
        payload.tip_kome_saljes = data.tip_kome_saljes;
    } else {
        throw new Error('Morate navesti receiver_id ili tip_kome_saljes');
    }
    
    const response = await fetch(`${API_BASE}/Obavestenje`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri slanju obaveštenja: ${error}`);
    }
    return await response.text();
}

async function getObavestenja() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Obavestenje/GetObavestenja`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam obaveštenja');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function posaljiPorukuApi(data) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Poruke/Send/${data.razgovorId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data.tekst)
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri slanju poruke: ${error}`);
    }
    return await response.text();
}

async function getPoruke(razgovorId) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Poruke/GetPoruke/${razgovorId}`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam poruke');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function getRazgovore() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Razgovor/GetRazgovore`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam razgovore');
    }
    const text = await response.text();
    if (!text) {
        return [];
    }
    return JSON.parse(text);
}

async function kreirajRazgovor(id_recipient) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Razgovor/CreateRazgovor/${id_recipient}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        throw new Error('Greška pri kreiranju razgovora');
    }
    return await response.text();
}

async function kreirajPrijavu(data) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Prijave/CreatePrijavu?id_izvodjaca=${data.id_izvodjaca}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ razlog: data.razlog })
    });
    if (!response.ok) {
        throw new Error('Greška pri slanju prijave');
    }
    return await response.text();
}

async function getPrijave() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Prijave/GetPrijave`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        if (response.status === 400) {
            console.log('Nema nerešenih prijava');
            return [];
        }
        throw new Error('Ne mogu da učitam prijave');
    }
    const text = await response.text();
    if (!text || text === "Nema prijava") {
        return [];
    }
    return JSON.parse(text);
}

async function obradiPrijavu(prijavaId, adminKomentar, blokiraj) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Prijave/ObradiPrijavu?blokiraj=${blokiraj}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id_prijave: prijavaId,
            admin_komentar: adminKomentar
        })
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri obradi prijave: ${error}`);
    }
    return await response.text();
}

async function getKlijentInfoForAdmin(id) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Klijenti/GetKlijent?id_klijent=${id}`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam podatke klijenta');
    }
    const text = await response.text();
    if (!text) {
        return null;
    }
    return JSON.parse(text);
}

async function updateKlijent(data) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Klijenti/UpdateKlijent`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data) 
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri ažuriranju klijenta: ${error}`);
    }
    return await response.text();
}

async function updateMajstor(data) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Majstor/UpdateMajstor`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data) 
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri ažuriranju majstora: ${error}`);
    }
    return await response.text();
}

async function updateFirma(data) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Firme/UpdateFirma`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data) 
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri ažuriranju firme: ${error}`);
    }
    return await response.text();
}

async function posaljiZahtev(zahtevData) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Zahtevi/SendRequest`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(zahtevData)
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri slanju zahteva');
    }
    return await response.text();
}

async function getZahtevi(stanje) {
    const token = getToken();
    let url = `${API_BASE}/Zahtevi/GetZahtevi`;
    if (stanje !== undefined && stanje !== null) {
        url += `?stanje=${stanje}`;
    }
    const response = await fetch(url, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam zahteve');
    }
    const text = await response.text();
    if (!text || text === "Nema zahteva") {
        return [];
    }
    return JSON.parse(text);
}

async function odgovoriNaZahtev(zahtevId, prihvati, podaci) {
    const token = getToken();
    const url = `${API_BASE}/Zahtevi/ZahtevAnswear?prihvati=${prihvati}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(podaci)
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri odgovoru na zahtev');
    }
    return await response.text();
}

async function getZakazivanja() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Zahtevi/GetZakazivanje`, {
        headers: getHeaders()
    });
    if (!response.ok) {
        throw new Error('Ne mogu da učitam zakazivanja');
    }
    const text = await response.text();
    if (!text || text === "Nema zakazivanja") {
        return [];
    }
    return JSON.parse(text);
}

async function odgovoriKlijentNaZakazivanje(zakazivanjeId, prihvati) {
    const token = getToken();
    const url = `${API_BASE}/Zahtevi/KlijentOdgovorNaZakazivanje?response=${prihvati}&idZakazivanja=${zakazivanjeId}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri odgovoru na zakazivanje');
    }
    return await response.text();
}

async function dodajGrad(nazivGrada) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Gradovi?naziv_grada=${encodeURIComponent(nazivGrada)}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri dodavanju grada: ${error}`);
    }
    return await response.text();
}

async function obrisiGrad(gradId) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Gradovi/${gradId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri brisanju grada: ${error}`);
    }
    return await response.text();
}

async function obrisiOcenu(ocenaId) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Ocena/DeleteOcenu/${ocenaId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri brisanju ocene: ${error}`);
    }
    return await response.text();
}

async function setKonacnaCena(idZakazivanja, konacnaCena) {
    const token = getToken();
    const url = `${API_BASE}/Zakazivanje/SetKonacna/${idZakazivanja}?konacna_cena=${konacnaCena}&id_zakazivanje=${idZakazivanja}`;
    
    console.log('Postavljam konačnu cenu (kartica):', url);
    
    const response = await fetch(url, {
        method: 'PUT',
        headers: getHeaders()
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri postavljanju cene: ${error}`);
    }
    return await response.text();
}

async function platiKesom(idZakazivanja, konacnaCena) {
    const token = getToken();
    const url = `${API_BASE}/Zakazivanje/PlacanjeKesom/${idZakazivanja}?konacna_cena=${konacnaCena}&id_zakazivanje=${idZakazivanja}`;
    
    console.log('Plaćanje kešom:', url);
    
    const response = await fetch(url, {
        method: 'PUT',
        headers: getHeaders()
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(`Greška pri plaćanju kešom: ${error}`);
    }
    return await response.text();
}

async function platiKarticom(idZakazivanja) {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Zakazivanje/CreateCheckoutSession?id_zakazivanja=${idZakazivanja}`, {
        method: 'POST',
        headers: getHeaders()
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri kreiranju Stripe sesije');
    }
    
    const data = await response.json();
    console.log('Stripe odgovor:', data);
    
    if (!data.url) {
        throw new Error('Stripe URL nije vraćen');
    }
    
    return data;
}

async function napraviStripeNalog() {
    const token = getToken();
    const response = await fetch(`${API_BASE}/Korisnici/MakeStripeNalog`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri kreiranju Stripe naloga');
    }
    const data = await response.json();
    return data;
}

async function dodajKategoriju(naziv, roditeljId) {
    const token = getToken();
    let url = `${API_BASE}/Kategorije/Napravi kategoriju`;
    if (roditeljId) {
        url += `?idRod=${roditeljId}`;
    } else {
        url += `?idRod=0`;
    }
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            naziv_kategorije: naziv
        })
    });
    
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Greška pri dodavanju kategorije');
    }
    
    return await response.text();
}