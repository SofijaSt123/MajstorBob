using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore;

namespace Majstor_bob.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        :base(options) { }  
        //povezujem sa bazom Dbset<ime modela> [ime table u sql]
        public DbSet<korisnici> korisnici { get; set; }
        public DbSet<gradovi> gradovi { get; set; }
        public DbSet<firme> firme { get; set; }
        public DbSet<gradovi_rada> gradovi_rada { get; set; }
        public DbSet<kategorije> kategorije { get; set; }
        public DbSet<klijenti> klijenti { get; set; }
        public DbSet<majstori> majstori { get; set; }
        public DbSet<obavestenja> obavestenja { get; set; }
        public DbSet<ocene> ocene { get; set; }
        public DbSet<poruke> porukes { get; set; }
        public DbSet<prijave> prijave { get; set; }
        public DbSet<pripada> pripada { get; set; }
        public DbSet<razgovor> razgovor { get; set; }
        public DbSet<zahtevi> zahtevi { get; set; }
        public DbSet<zakazivanje> zakazivanje { get; set; }
        public DbSet<zapisnik_admin > zapisnik_admin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //one to one sa korisnici razlika po ulozi admin nema svoju tabelu 

            //Korisnici
            //
            modelBuilder.Entity<korisnici>().HasKey(k => k.id_korisnik);

            modelBuilder.Entity<korisnici>()
            .HasIndex(k => k.email)
            .IsUnique();
            
            //MAJSTORI
            //
            modelBuilder.Entity<majstori>().HasKey(m => m.id_majstora);

            modelBuilder.Entity<majstori>()
            .HasIndex(m=>m.jmbg)
            .IsUnique();
            /*
            modelBuilder.Entity<majstori>()
                .HasOne<korisnici>()   //1 red majstori povezan sa 1 red korisnici
                .WithOne()             // nastavak povezna sa <korisnici> korisnik 1 red 1 majstor
                .HasForeignKey<majstori>(m => m.id_majstora)
                .HasPrincipalKey<korisnici>(k => k.id_korisnik);
            */
            modelBuilder.Entity<majstori>()
                .HasOne(m=>m.korisnik)   //1 red majstori povezan sa 1 red korisnici
                .WithOne(k=>k.majstori)             // nastavak povezna sa <korisnici> korisnik 1 red 1 majstor
                .HasForeignKey<majstori>(m => m.id_majstora)
                .HasPrincipalKey<korisnici>(k => k.id_korisnik);

            //FIRME
            //
            modelBuilder.Entity<firme>().HasKey(f => f.id_firme);

            modelBuilder.Entity<firme>()
            .HasIndex(f => f.pib)
            .IsUnique();

            modelBuilder.Entity<firme>()
                .HasOne(f=>f.korisnik)
                .WithOne(k=>k.firme)
                .HasForeignKey<firme>(f => f.id_firme)
                .HasPrincipalKey<korisnici>(k => k.id_korisnik);

            //KLIJENTI
            //
            modelBuilder.Entity<klijenti>().HasKey(k => k.id_klijent);

            modelBuilder.Entity<klijenti>()
                .HasOne(k=>k.korisnik)
                .WithOne(k=>k.klijenti) 
                .HasForeignKey<klijenti>(k => k.id_klijent)
                .HasPrincipalKey<korisnici>(k => k.id_korisnik);

            modelBuilder.Entity<klijenti>()
              .HasOne<gradovi>()
              .WithOne()
              .HasForeignKey<klijenti>(k => k.id_grad_rada)
              .HasPrincipalKey<gradovi>(g => g.id_grad);

            //OBAVESTENJA
            //
            modelBuilder.Entity<obavestenja>().HasKey(o => o.id);

            modelBuilder.Entity<obavestenja>()
              .HasOne(o => o.sender)
              .WithMany(k => k.obavestenja_posiljac)
              .HasForeignKey(o => o.admin_id)
              .HasPrincipalKey(k => k.id_korisnik)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<obavestenja>()
                .HasOne(o => o.receiver)
                .WithMany(k => k.obavestenje_primalac)
                .HasForeignKey(o => o.receiver_id)
                .HasPrincipalKey(k => k.id_korisnik)
                .OnDelete(DeleteBehavior.Restrict);

            //GRADOVI
            //
            modelBuilder.Entity<gradovi>().HasKey(g => g.id_grad);

            modelBuilder.Entity<gradovi>()
            .HasIndex(g => g.naziv_grada)
            .IsUnique();

            //GRADOVI RADA
            //
            modelBuilder.Entity<gradovi_rada>().HasKey(g => g.id_grad_rada);

            modelBuilder.Entity<gradovi_rada>()
            .HasOne(gr => gr.grad)
            .WithMany(g => g.gradovi_rad)
            .HasForeignKey(gr => gr.id_grada)
            .HasPrincipalKey(g => g.id_grad); //cisto informativno znam da ne mora al ne razumem dovoljno da znam kad ne mora

            modelBuilder.Entity<gradovi_rada>()
            .HasOne(gr => gr.Izvodjac)
            .WithMany(k => k.gradovi_rad)
            .HasForeignKey(gr => gr.id_izvodjaca)
            .HasPrincipalKey(k => k.id_korisnik);

            //KATEGORIJE
            //
            modelBuilder.Entity<kategorije>().HasKey(ka => ka.id_kategorije);

            modelBuilder.Entity<kategorije>()
            .HasIndex(k=>k.naziv_kategorije)
            .IsUnique();

            modelBuilder.Entity<kategorije>()
            .HasOne(k => k.roditelj)
            .WithMany(k => k.pod_kategorija)
            .HasForeignKey(k => k.id_roditelja)
            .HasPrincipalKey(k => k.id_kategorije)
            .OnDelete(DeleteBehavior.Cascade);


            //PRIPADAD
            //
            modelBuilder.Entity<pripada>().HasKey(p => p.id_pripada);

            modelBuilder.Entity<pripada>()
            .HasOne(p => p.izvodjac)
            .WithMany(k => k.kategorija_kojoj_pripada)
            .HasForeignKey(p => p.id_izvodjaca)
            .HasPrincipalKey(k => k.id_korisnik)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<pripada>()
            .HasOne(p => p.kategorija)
            .WithMany(k => k.kategorije_izvodjaca)
            .HasForeignKey(p => p.id_kategorije)
            .HasPrincipalKey(k => k.id_kategorije)
            .OnDelete(DeleteBehavior.Cascade);

            //OCENA
            //
            modelBuilder.Entity<ocene>().HasKey(o => o.id_ocene);

            modelBuilder.Entity<ocene>()
            .HasOne(o => o.klijent)
            .WithMany(k => k.ocene)
            .HasForeignKey(o => o.id_klijenta)
            .HasPrincipalKey(k => k.id_klijent);

            modelBuilder.Entity<ocene>()
            .HasOne(o => o.izvodjac)
            .WithMany(k => k.ocenjen_izvodjac)
            .HasForeignKey(o => o.id_izvodjaca)
            .HasPrincipalKey(k => k.id_korisnik);

            //PORUKE
            //
            modelBuilder.Entity<poruke>().HasKey(p => p.id_poruke);

            modelBuilder.Entity<poruke>()
            .HasOne(p => p.razgovor)
            .WithMany(r => r.poruke)
            .HasForeignKey(o => o.id_razgovora)
            .HasPrincipalKey(r=>r.id_razgovora);

            modelBuilder.Entity<poruke>()
            .HasOne(p => p.posiljac)
            .WithMany()
            .HasForeignKey(p => p.posiljac_id)
            .HasPrincipalKey(k => k.id_korisnik);

            //PRIJAVE
            //
            modelBuilder.Entity<prijave>().HasKey(p => p.id_prijave);

            modelBuilder.Entity<prijave>()
            .HasOne(p => p.izvodjac)
            .WithMany(k => k.prijavljena_osoba)
            .HasForeignKey(p => p.id_prijavljena_osoba)
            .HasPrincipalKey(k => k.id_korisnik);

            modelBuilder.Entity<prijave>()
            .HasOne(p => p.klijent)
            .WithMany(k => k.ko_prijavljuje)
            .HasForeignKey(p => p.id_prijavljaca)
            .HasPrincipalKey(k => k.id_klijent);

            //RAZGOVOR
            //
            modelBuilder.Entity<razgovor>().HasKey(r => r.id_razgovora);

            modelBuilder.Entity<razgovor>()
            .HasOne(r => r.klijent)
            .WithMany(k => k.razgovors)
            .HasForeignKey(r => r.id_klijent)
            .HasPrincipalKey(k => k.id_klijent);

            modelBuilder.Entity<razgovor>()
            .HasOne(r => r.izvodjac)
            .WithMany() 
            .HasForeignKey(r => r.id_majstor)
            .HasPrincipalKey(k => k.id_korisnik);

            //ZAHTEVI
            modelBuilder.Entity<zahtevi>().HasKey(z => z.id_zahteva);

            modelBuilder.Entity<zahtevi>()
            .HasOne(z => z.klijent)
            .WithMany(k => k.sender_zahteva)
            .HasForeignKey(z => z.id_klijenta)
            .HasPrincipalKey(k => k.id_klijent);

            modelBuilder.Entity<zahtevi>()
            .HasOne(z => z.izvodjac)
            .WithMany(k => k.kome_salje_zahtev)
            .HasForeignKey(z => z.id_izvodjaca)
            .HasPrincipalKey(k => k.id_korisnik);

            //ZAKAZIVANJE
            //
            modelBuilder.Entity<zakazivanje>().HasKey(z => z.id_zakazivanja);

            modelBuilder.Entity<zakazivanje>()
            .HasOne(z => z.zahtev)
            .WithMany(z => z.zakazivanja)
            .HasForeignKey(z => z.id_zahteva);

            //ZAPISNIK_ADMIN
            //
            modelBuilder.Entity<zapisnik_admin>().HasKey(za => za.id_zapisnik);

            modelBuilder.Entity<zapisnik_admin>()
            .HasOne(za => za.admin)
            .WithMany(k => k.zapisnik)
            .HasForeignKey(za=>za.id_admina);

            }
    }
}
//modelBuilder.Entity<ime modela>() -koja tabela
//.HasMany(e=>e.Posts) -Blog ima vise posta IColection<Posts
//.WithOne(e=>e.Blog) svaki post pripada 1 blogu Blog Blog
//.HasForeignKey(e => e.BlogId) -Fk u posts je blogId
//.HasPrincipalKey(e => e.Id); //Posts.BlogId → pokazuje na Blog.Id
//.HasKey(e=>e.id_posts) -ovo je primarni kljuc
//.HasOne(e=>e.Blog) - BLog Blog
//WithOne() -veza 1:1 korisnici i klijejnt
/*
 HasAlternateKey()	Configures an alternate key in the EF model for the entity.
HasIndex()	Configures an index on the specified properties.
HasKey()	Configures the property or list of properties as Primary Key.
HasMany()	Configures the Many part of the relationship, where an entity contains the reference collection property of another type for one-to-Many or many-to-many relationships.
HasOne()	Configures the One part of the relationship, where an entity contains the reference property of another type for one-to-one or one-to-many relationships.
Ignore()	Configures that the class or property should not be mapped to a table or column.
OwnsOne()	Configures a relationship where the target entity is owned by this entity. The target entity key value is propagated from the entity it belongs to.
ToTable()	Configures the database table that the entity maps to.

HasColumnName()	Configures the corresponding column name in the database for the property.
HasColumnType()	Configures the data type of the corresponding column in the database for the property.
HasComputedColumnSql()	Configures the property to map to a computed column in the database when targeting a relational database.
HasDefaultValue()	Configures the default value for the column that the property maps to when targeting a relational database.
HasDefaultValueSql()	Configures the default value expression for the column that the property maps to when targeting relational database.
HasField()	Specifies the backing field to be used with a property.
HasMaxLength()	Configures the maximum length of data that can be stored in a property.
IsConcurrencyToken()	Configures the property to be used as an optimistic concurrency token.
IsRequired()	Configures whether the valid value of the property is required or whether null is a valid value.
IsRowVersion()	Configures the property to be used in optimistic concurrency detection.
IsUnicode()	Configures the string property which can contain Unicode characters or not.
ValueGeneratedNever()	Configures a property which cannot have a generated value when an entity is saved.
ValueGeneratedOnAdd()	Configures that the property has a generated value when saving a new entity.
ValueGeneratedOnAddOrUpdate()	Configures that the property has a generated value when saving a new or existing entity.
ValueGeneratedOnUpdate()	Configures that a property has a generated value when saving an existing entity.
 he OnDelete() method cascade delete behavior uses the DeleteBehavior parameter. You can specify any of the following DeleteBehavior values, based on your requiremen
Cascade : Dependent entities will be deleted when the principal entity is deleted.
ClientSetNull: The values of foreign key properties in the dependent entities will be set to null.
Restrict: Prevents Cascade delete.
SetNull: The values of foreign key properties in the dependent entities will be set to null.

 */