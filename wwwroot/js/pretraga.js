let izabranaKategorijaTekst = '';

async function napuniKategorijeMeni() {
    try {
        const kategorije = await getKategorije();
        const select = document.getElementById('filterKategorija');
        select.innerHTML = '<option value="" disabled selected>-- Izaberite vrstu usluge --</option>';
        
        const glavne = kategorije.filter(k => k.id_roditelja === null);
        
        for (let g of glavne) {
            const glavnaOpcija = document.createElement('option');
            glavnaOpcija.value = '';
            glavnaOpcija.textContent = '▸ ' + g.naziv_kategorije;
            glavnaOpcija.disabled = true;
            glavnaOpcija.style.fontWeight = 'bold';
            glavnaOpcija.style.color = '#C13B27';
            select.appendChild(glavnaOpcija);
            
            const podkategorije = kategorije.filter(k => k.id_roditelja === g.id_kategorije);
            
            for (let p of podkategorije) {
                const potomci = kategorije.filter(k => k.id_roditelja === p.id_kategorije);
                
                if (potomci.length > 0) {
                    const podOpcija = document.createElement('option');
                    podOpcija.value = '';
                    podOpcija.textContent = '    ▸ ' + p.naziv_kategorije;
                    podOpcija.disabled = true;
                    podOpcija.style.fontWeight = 'bold';
                    podOpcija.style.color = '#A9321F';
                    select.appendChild(podOpcija);
                    
                    for (let u of potomci) {
                        const option = document.createElement('option');
                        option.value = u.id_kategorije;
                        option.textContent = '        ' + u.naziv_kategorije;
                        select.appendChild(option);
                    }
                } else {
                    const option = document.createElement('option');
                    option.value = p.id_kategorije;
                    option.textContent = '    ' + p.naziv_kategorije;
                    select.appendChild(option);
                }
            }
        }
        
    } catch (error) {
        console.error('Greška pri učitavanju kategorija za meni:', error);
    }
}

async function ucitajGradoveZaFilter() {
    try {
        const gradovi = await getGradovi();
        const select = document.getElementById('filterGrad');
        select.innerHTML = '<option value="" disabled selected>-- Izaberite grad --</option>';
        
        for (let g of gradovi) {
            const option = document.createElement('option');
            option.value = g.id_grad;
            option.textContent = g.naziv_grada.charAt(0).toUpperCase() + g.naziv_grada.slice(1);
            select.appendChild(option);
        }
    } catch (error) {
        console.error('Greška pri učitavanju gradova:', error);
    }
}

function prikaziMajstore(majstori, kategorijaNaziv, doplataFilter) {
    const tbody = document.getElementById('rezultatiPretrage');
    
    if (!majstori || majstori.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center py-4">Nema pronađenih majstora</td></tr>';
        return;
    }
    
    // UZMI GRAD IZ FILTERA
    const gradSelect = document.getElementById('filterGrad');
    const gradNaziv = gradSelect.options[gradSelect.selectedIndex]?.text || 'Nepoznat';
    
    const prikaziDoplatu = doplataFilter === 'da' ? 'Da' : 'Ne';
    const doplataKlasa = doplataFilter === 'da' ? 'neverifikovan-badge' : 'verifikovan-badge';
    
    let html = '';
    for (let i = 0; i < majstori.length; i++) {
        const m = majstori[i];
        
        let imePrezime = m.ime || 'Nepoznato';
        if (m.nazivFirme) {
            imePrezime = m.nazivFirme;
        }
        
        const kategorijaPrikaz = m.kategorija || kategorijaNaziv || 'Majstor';
        const prikazanaOcena = m.prosekOcena || 0;
        const profilId = m.id || m.id_izvodjaca;
        
        html += `
            <tr>
                <td class="tekst-crven">${imePrezime}</td>
                <td class="tekst-crven">${kategorijaPrikaz}</td>
                <td class="tekst-crven">${gradNaziv}</td>
                <td class="text-center"><span class="${doplataKlasa}">${prikaziDoplatu}</span></td>
                <td class="text-center">${prikazanaOcena.toFixed(1)} <i class="fa-solid fa-star tekst-zut"></i></td>
                <td class="text-center">
                    <button class="btn btn-sm dugme" onclick="prikaziProfilMajstora(${profilId})">
                        <i class="fa-solid fa-eye"></i> Profil
                    </button>
                </td>
            </tr>
        `;
    }
    tbody.innerHTML = html;
}

async function filtrirajMajstore() {
    const kategorijaId = document.getElementById('filterKategorija').value;
    const selectElement = document.getElementById('filterKategorija');
    const izabranaKategorijaTekst = selectElement.options[selectElement.selectedIndex]?.text || '';
    const gradId = document.getElementById('filterGrad').value;
    const doplataFilter = document.getElementById('filterDoplata').value;
    const minOcena = parseFloat(document.getElementById('filterOcena').value);
    
    if (!kategorijaId) {
        document.getElementById('rezultatiPretrage').innerHTML = `
            <tr><td colspan="6" class="text-center py-4 tekst-crven">
                <i class="fa-solid fa-circle-exclamation fa-2x mb-2"></i>
                <p>Morate izabrati vrstu usluge pre pretrage!</p>
            </td></tr>
        `;
        return;
    }
    
    if (!gradId) {
        document.getElementById('rezultatiPretrage').innerHTML = `
            <tr><td colspan="6" class="text-center py-4 tekst-crven">
                <i class="fa-solid fa-circle-exclamation fa-2x mb-2"></i>
                <p>Morate izabrati grad pre pretrage!</p>
            </td></tr>
        `;
        return;
    }
    
    if (!doplataFilter) {
        document.getElementById('rezultatiPretrage').innerHTML = `
            <tr><td colspan="6" class="text-center py-4 tekst-crven">
                <i class="fa-solid fa-circle-exclamation fa-2x mb-2"></i>
                <p>Morate izabrati opciju za doplatu!</p>
            </td></tr>
        `;
        return;
    }
    
    document.getElementById('rezultatiPretrage').innerHTML = '<tr><td colspan="6" class="text-center py-4">Učitavanje majstora...</td></tr>';
    
    try {
        const doplataBool = doplataFilter === 'da' ? true : false;
        
        const majstori = await pretraziIzvodjace(kategorijaId, gradId, minOcena, doplataBool);
        console.log('Pronađeni majstori:', majstori);
        
        prikaziMajstore(majstori, izabranaKategorijaTekst, doplataFilter);
        
    } catch (error) {
        console.error('Greška:', error);
        document.getElementById('rezultatiPretrage').innerHTML = `
            <tr><td colspan="6" class="text-center py-4 tekst-crven">
                <i class="fa-solid fa-circle-exclamation fa-2x mb-2"></i>
                <p>Greška pri učitavanju majstora. Pokušajte ponovo.</p>
            </td></tr>
        `;
    }
}

function prikaziProfilMajstora(id) {
    if (!id || id === 'undefined') {
        alert('ID majstora nije dostupan');
        return;
    }
    window.location.href = `majstor_profil.html?id=${id}`;
}

document.addEventListener('DOMContentLoaded', function() {
    napuniKategorijeMeni();
    ucitajGradoveZaFilter();
});

document.getElementById('pretraziDugme').onclick = filtrirajMajstore;