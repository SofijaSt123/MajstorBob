using AutoMapper;
using Majstor_bob.Dto;
using Majstor_bob.Models;
using Majstor_bob.Repository;

namespace Majstor_bob.Helper
{
    public class MapingProfiles : Profile
    {
        public MapingProfiles() 
        {
            CreateMap<KorisniciDto, korisnici>();
            CreateMap<korisnici, KorisniciDto>();
            CreateMap<korisnici, RegisterKorisnikDto>();
            CreateMap<RegisterKorisnikDto, korisnici>();

            

            CreateMap<kategorije, KategorijeDto>();
            CreateMap<KategorijeDto, kategorije>();

            CreateMap<klijenti, KlijentDto>();
            CreateMap<majstori, MajstoriDto>();
            
            CreateMap<firme, FirmeDto>();
            CreateMap<firme, UpdateFirmeDto>();
            CreateMap<FirmeDto, firme>();
            CreateMap<UpdateFirmeDto, firme>();

            CreateMap<ocene, OceneDto>();

            CreateMap<gradovi,GradoviDto>();
            CreateMap<GradoviDto,gradovi>();

            CreateMap<gradovi_rada, GradoviRadaDto>();

            CreateMap<zahtevi, ZahtevDto>();
            CreateMap<zahtevi,ListaZahtevaDto>();

            CreateMap<zakazivanje, SendZakazivanjeDto>();
            CreateMap<zakazivanje, ListaZakazivanjaDto>();

            CreateMap<obavestenja, CreateObavestenjaDto>();
            CreateMap<obavestenja, GetObavestenjaDto>();
            CreateMap<GetObavestenjaDto, obavestenja>();

            CreateMap<prijave, CreatePrijaveDto>();
            CreateMap<prijave, GetPrijaveDto>();
            CreateMap<GetPrijaveDto, prijave>();
            CreateMap<prijave, UpdatePrijavaDto>();

            CreateMap<poruke, CreatePorukeDto>();
            CreateMap<poruke, GetPorukeDto>();
            CreateMap<GetPorukeDto, poruke>();

            CreateMap<ocene, OceneDto>();

            CreateMap<gradovi_rada, GetGradoviRadaDto>();






        }
    }
}
