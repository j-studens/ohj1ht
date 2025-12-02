using System;
using Jypeli;
using Jypeli.Assets;

/// @author Seppänen Joonas
/// @version 1.12.2025
/// <summary>
/// LUMIUKKO FIGHTER on tasohyppelypeli, jossa pelaaja torjuu joulupukin sotajoukkoja ja pyrkii kukistamaan itse joulupukin.
/// TAVOITE: kerää mahdollisimman paljon pisteitä ja kukista joulupukki.
/// Pelissä kerätään lahjoja ja nyrkkeilään vihollisia vastaan, mikä kasvattaa pistemäärää.
/// Liikkuminen tapahtuu nuolinäppäinten ja nyrkkeily välilyönnin avulla. 
/// Pelissä on useita pelikenttiä ja haasteita, kuten lumisade.
/// Kaiken lisäksi pelissä on toimiva graafinen käyttöliittymä.
/// </summary>
public class Ohj1htPeli : PhysicsGame
{
    
    // Piste-attribuutit.    
    private const int PisteRaja1 = 4;
    private const int PisteRaja2 = 7;
    private const int PisteRaja3 = 9;
    private const int PisteRaja4 = 11;
    private const int PisteRaja5 = 13;
    private const int PisteRaja6 = 15;
    private IntMeter _pelaajanPisteet;
    private int[] _kokonaisPisteet = { 0, 0, 0 };
    
    // Kenttä attribuutti.
    private int _kenttaNro = 1;
    private int _viimeinenKentta = 3;
    private int _toinenKentta = 2;
    private bool _aiheuttikoKuolema;
    private bool _aiheuttikoVoitto;
    
    // Fysiikka-attribuutit.
    private const double Nopeus = 200;
    private const double HyppyNopeus = 750;
    private const int RuudunKoko = 40;
    private const int VihollinenMitta = 20;
    private const int JoulupukkiMitta = 50;
    
    // Kenttätaulukko.
    private static readonly string[] KenttaTaulukko ={"kentta1.txt","kentta2.txt","kentta3.txt"};
    
    // Kuvataulukot. 
    private Image[] _kuvaTaulukko = {LoadImage("ilkeä_ukko.png"),LoadImage("jere_default.png"),LoadImage("lahja.png"),LoadImage("joulupukki.png")};
    private Image[] _nyrkkeilyTaulukko = {LoadImage("vasen_nyrkki.png"),LoadImage("oikea_nyrkki.png"),LoadImage("jere_ilkeä.png")};
    private Image[] _lumiHiutaleTaulukko = {LoadImage("lumihiutale.png"),LoadImage("lumihiutale2.png"),LoadImage("lumihiutale3.png"),LoadImage("lumihiutale4.png"),LoadImage("lumihiutale5.png")};
    
    // Valikkotaulukot
    private string[] _alkuvaihtoehdot = { "Aloita peli", "Lopeta" };
    private string[] _toisetvaihtoehdot = { "Jatka peli", "Lopeta" };
    private string[] _uudetvaihtoehdot = { "Uusi peli", "Lopeta" };
    
    // Olio-attribuutit
    private PlatformCharacter _pelaaja1;
    private PlatformCharacter _vihollinen;
    private PlatformCharacter _joulupukki;
    private PlatformCharacter _nyrkki;
    private PhysicsObject _lumihiutale;
    Random _satunnainen = new Random();

    /// <summary>
    /// Syöttöpiste.
    /// </summary>
    public override void Begin()
    {
        LisaaValikko(_alkuvaihtoehdot);
    }
    
    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka luo alkuvalikon.
    /// </summary>
    /// <param name="vaihtoehdot"> Valikkonäppäinten vaihtoehdot valikkoon </param>
    private void LisaaValikko(string[] vaihtoehdot)
    {
        
        int valikkoY = 0;
        Color valikkoVari = Color.DarkGreen;
        
        // Toteuttaa muulloin kuin pelaajan kuoleman tai voiton seurauksena.
        if (!_aiheuttikoKuolema && !_aiheuttikoVoitto)
        {
            // Poistetaan kaikki. Asetetaan uusi taustaväri
            ClearAll();
            Level.Background.CreateGradient(Color.LightBlue, Color.DarkBlue);   
            
            // Luodaan otsikko
            Label otsikko = LuoOtsikko("LUMIUKKO FIGHTER", "", 180, Color.White, 100, true);
            Add(otsikko);
            
        }
        
        // Toteuttaa, jos aliohjelma kutsuttiin pelaajan kuoleman seurauksena.
        if (_aiheuttikoKuolema)
        {
            valikkoY = -125;
            valikkoVari = Color.Red;
        }
        
        // Toteuttaa, jos aliohjelma kutsuttiin voiton seurauksena.
        if (_aiheuttikoVoitto)
        {
            valikkoY = -125;
            valikkoVari = Color.Green;
            vaihtoehdot = _uudetvaihtoehdot;
        }
        
        // Luodaan näppäimet valikolle.
        MultiSelectWindow valikko = LuoValikko(vaihtoehdot, valikkoY, valikkoVari);
        Add(valikko);
        
    }
    
    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka palauttaa valikon näppäimet.
    /// </summary>
    /// <param name="vaihtoehdot"> Valikkonäppäinten vaihtoehdot valikkoon. </param>
    /// <param name="valikkoY"> Näppäinvalikon sijainti Y-akselilla. </param>
    /// <param name="valikkoVari"> Näppäinvalikon väri. </param>
    /// <returns> Palauttaa näppäinvalikon. </returns>
    private MultiSelectWindow LuoValikko(string [] vaihtoehdot, int valikkoY, Color valikkoVari)
    {
        
        MultiSelectWindow valikko = new MultiSelectWindow("", vaihtoehdot);
        valikko.X = 0;
        valikko.Y = valikkoY;
        valikko.Color = valikkoVari;
        valikko.CapturesMouse = false;
        valikko.AddItemHandler(0, KenttaSeuraava);
        valikko.AddItemHandler(1, Exit);
        return valikko;

    }
    
    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka kutsuu ohjelman LisaaValikko, mutta antaa toisenlaisen argumentin.
    /// Mitä tämä saavuttaa: pelin valikossa näkyy "Aloita peli" sijaan "Jatka peli".
    /// Kenttänumero jää kuitenkin talteen.
    /// </summary>
    private void ValikkoSeuraava()
    {
        LisaaValikko(_toisetvaihtoehdot);
    }
    
    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka kutsuu toisen aliohjelman: SeuraavaKentta. 
    /// </summary>
    private void KenttaSeuraava()
    {
        SeuraavaKentta();
    }

    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka käsittelee kuolematapausta. Luo erilaisen valikon.
    /// </summary>
    private void OletKuollut()
    {
        // Poistetaan kaikki ja asetetaan uusi taustaväri.
        ClearAll();
        Level.Background.CreateGradient(Color.Red, Color.DarkRed);  
        
        // Luodaan otsikko.
        Label otsikko = LuoOtsikko("KUOLIT", "", 50, Color.Black, 100, true);
        Add(otsikko);
        
        _aiheuttikoKuolema = true;

        // Kumotaan pisteet.
        KumoaPisteet();
        
        // Kutsutaan valikko.
        Timer.CreateAndStart(0.5, ValikkoSeuraava);
    }
    
    
    /// <summary>
    /// Kategoria: VALIKKO | Aliohjelma, joka käsittelee kuolematapausta. Luo erilaisen valikon
    /// </summary>
    private void OletVoittanut()
    {
        // Poistetaan kaikki ja asetetaan uusi taustaväri
        ClearAll();
        Level.Background.CreateGradient(Color.Green, Color.DarkGreen);  
        
        // Luodaan otsikko
        Label otsikko = LuoOtsikko("VOITTO!", "", 50, Color.White, 100, true);
        Add(otsikko);
        
        _aiheuttikoVoitto = true;
        
        // Lasketaan kokonaispisteet, resetoidaan kenttänumero, kutsutaan valikko
        TallennaPisteet();
        LaskeKaikkiPisteet();
        
        _kenttaNro = 1;
        
        Timer.CreateAndStart(0.5, ValikkoSeuraava);
    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohejlma, joka luo varsinaisen pelikentän.
    /// </summary>
    /// <param name=  "seuraavakentta"> Jonkin tietyn kentän .txt-tiedosto. </param> 
    private void LuoKentta(string seuraavakentta)
    {
        // Painovoima & fysiikka
        Gravity = new Vector(0, -1000);
        
        // Asettaa muuttujat lepotilaan, jotta valikkojen ulkonäkö pysyisi järkevänä.
        _aiheuttikoKuolema = false;
        _aiheuttikoVoitto = false;
            
        // Pelikentän omaisuudet.
        TileMap kentta = TileMap.FromLevelAsset(seuraavakentta);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaLahja);
        kentta.SetTileMethod('@', LisaaVihollinen);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.SetTileMethod('J', LisaaJoulupukki);
        kentta.Execute(RuudunKoko, RuudunKoko);
        
        // Kutsut
        LisaaNappaimet();
        LuoLaskuri();
        LisaaReunat();
        LisaaZoomaus();

    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Ohjelma, joka luo tason. Kutsuttava.
    /// </summary>
    /// <param name="paikka"> Sijaintikoordinaatti kentällä. </param>
    /// <param name="leveys"> Olion Leveysmitta. </param>
    /// <param name="korkeus"> Olion korkeusmitta. </param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.White;
        taso.Tag = "taso";
        Add(taso);
    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohjelma, joka asettaa zoomauksen pelaajaan.
    /// </summary>
    private void LisaaZoomaus()
    {
        Camera.Follow(_pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohjelma, joka asettaa reunat.
    /// </summary>
    private void LisaaReunat()
    {
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkBlue, Color.SkyBlue);
        Surface alareuna = Surface.CreateBottom(Level, 30, 100, 40);
        Add(alareuna);
    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohjelma, joka vastaa kentänvaihdosta. Kutsuttava.
    /// </summary>
    private void SeuraavaKentta()
    {
        
        // Poistaa kaiken.
        ClearAll();
        
        // Valitsee kenttätiedoston taulukosta.
        switch (_kenttaNro)
        {
            case 1:
                LuoKentta(KenttaTaulukko[0]);
                break;
            case 2:
                LuoKentta(KenttaTaulukko[1]);
                break;
            case 3:
                LuoKentta(KenttaTaulukko[2]);
                break;
            case 4:
                break;
        }
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka asettaa ohjaimet peliin. Kutsuttava.
    /// Nuolinäppäimet: liikkuminen & hyppääminen. Välilyönti: nyrkkeileminen
    /// </summary>
    private void LisaaNappaimet()
    {
        // Nuolinäppäimet ja välilyönti.
        Keyboard.Listen(Key.Space, ButtonState.Pressed, LisaaNyrkki, "Nyrkkeile", _pelaaja1, -Nopeus);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liiku", _pelaaja1, -Nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liiku", _pelaaja1, Nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", _pelaaja1, HyppyNopeus);
        
        
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ValikkoSeuraava, "Takaisin Valikkoon");
    }

    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka käsittelee liikkumisen toiminnallisuutta. Kutsuttava.
    /// </summary>
    /// <param name="hahmo"> Olio, johon toiminnallisuus kohdistuu </param>
    /// <param name="nopeus"> Vaikuttavan voiman ominaisuus </param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka käsittelee hyppyjen toiminnallisuutta. Kutsuttava.
    /// </summary>
    /// <param name="hahmo"> Olio, johon toiminnallisuus kohdistuu </param>
    /// <param name="nopeus"> Vaikuttavan voiman ominaisuus </param>
    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka lisää nyrkkiolion, jolla on lyhyt elinikä. Kutsuttava.
    /// </summary>
    /// <param name="hahmo"> Olio, johon toiminnallisuus kohdistuu </param>
    /// <param name="nopeus"> Vaikuttavan voiman ominaisuus </param>
    private void LisaaNyrkki(PlatformCharacter hahmo, double nopeus)
    {

        // Oikea nyrkki.
        PlatformCharacter nyrkki1 = LuoNyrkki(hahmo, -20, 1);
        
        // Vasen nyrkki.
        PlatformCharacter nyrkki2 = LuoNyrkki(hahmo, 20, 0);
    
        Add(nyrkki1);
        Add(nyrkki2);
        
        // Pelaajahahmon ilme muuttuu.
        _pelaaja1.Image = _nyrkkeilyTaulukko[2];
        
        // Hahmon ilme muuttuu takaisin normaaliksi.
        Timer.CreateAndStart(1, JerenIlme);
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Alihojelma, joka palauttaa nyrkkiolion. 
    /// </summary>
    /// <param name="hahmo"> Olio, jolle luodaan nyrkkioliot. </param>
    /// <param name="offSet"> Nyrkkiolion sijainnin poikkeama. </param>
    /// <param name="taulukkoIndeksi"> Nyrkkiolion grafiikat. </param>
    /// <returns> Palauttaa nyrkkiolion. </returns>
    private PlatformCharacter LuoNyrkki(PlatformCharacter hahmo, int offSet, int taulukkoIndeksi)
    {
        _nyrkki = new PlatformCharacter(60, 60);
        _nyrkki.X = hahmo.X + offSet;
        _nyrkki.Y = hahmo.Y;
        _nyrkki.Image = _nyrkkeilyTaulukko[taulukkoIndeksi];
        _nyrkki.Tag = "nyrkki";
        _nyrkki.LifetimeLeft = TimeSpan.FromSeconds( 0.05 );
        
        return _nyrkki;
        
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka muuttaa pelaajahahmon ilmeen.
    /// </summary>
    private void JerenIlme()
    {
         _pelaaja1.Image = _kuvaTaulukko[1]; 
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka käsittelee nyrkkitörmäystapausta.
    /// Mitä tapahtuu: kasvattaa pistemäärän, räjähdys ilmenee, vihollinen kuolee.
    /// </summary>
    /// <param name="vihollinen"> Olio, jota tuhotaan. </param>
    /// <param name="nyrkki"> Olio, jonka seurauksena tuhottava olio tuhotaan.</param>
    private void NyrkkiTormays(PhysicsObject vihollinen, PhysicsObject nyrkki)
    {
        _pelaajanPisteet.Value += 1;
        
        // Räjähdysolion kutsu.
        Explosion rajahdys = LuoRajahdys(vihollinen);
        Add(rajahdys);
        
        vihollinen.Destroy();
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka käsittelee nyrkkitörmäystapausta joulupukin suhteen.
    /// Mitä tapahtuu: voittotapahtuma ja voittovalikko ilmestyy.
    /// </summary>
    /// <param name="joulupukki"> Olio, jota tuhotaan. </param>
    /// <param name="nyrkki"> Olio, jonka seurauksena tuhottava olio tuhotaan. </param>
    private void NyrkkiTormaysJoulupukkiin(PhysicsObject joulupukki, PhysicsObject nyrkki)
    {
        _pelaajanPisteet.Value += 1;
        
        // Räjähdysolion kutsu.
        Explosion rajahdys = LuoRajahdys(joulupukki);
        Add(rajahdys);
        joulupukki.Destroy();
        
        //Voitto
        Timer.CreateAndStart(0.3, OletVoittanut);
    }
    
    
    /// <summary>
    /// Kategoria: PELAAJA | Aliohjelma, joka luo pelaajahahmon.
    /// Samalla luo törmäysehdot
    /// </summary>
    /// <param name="paikka"> Pelaajahahmon määritetty sijainti kentällä </param>
    /// <param name="leveys"> Pelaajahahmon leveysmitta </param>
    /// <param name="korkeus"> Pelaajahahmon korkeusmitta </param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        _pelaaja1 = new PlatformCharacter(leveys, korkeus);
        _pelaaja1.Position = paikka;
        _pelaaja1.Mass = 4.0;
        _pelaaja1.Image = _kuvaTaulukko[1];
        
        // Törmäysehdot: vihollinen & lahja
        AddCollisionHandler(_pelaaja1, "Lahja", LahjaTormays);
        AddCollisionHandler(_pelaaja1, "vihollinen", VihollinenTormays);
        
        Add(_pelaaja1);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka vastaa vihollisen törmäystapausta seuraavat tapahtumat.
    /// Mitä tapahtuu: räjähdys, pelaajahahmo kuolee, valikko ilmestyy.
    /// </summary>
    /// <param name="pelaaja"> Olio, jota tuhotaan. </param>
    /// <param name="vihollinen"> Olio, jonka seurauksena tuhottava olio tuhotaan. </param>
    private void VihollinenTormays(PhysicsObject pelaaja, PhysicsObject vihollinen)
    {
        
        // Räjähdysolion kutsu.
        Explosion rajahdys = LuoRajahdys(pelaaja);
        Add(rajahdys);
        
        pelaaja.Destroy();
        
        Timer.CreateAndStart(0.3, OletKuollut);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo vihollisen kenttään.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    /// <param name="paikka"> Vihollisolion määritetty sijainti kentällä </param>
    /// <param name="leveys"> Vihollisolion leveysmitta </param>
    /// <param name="korkeus"> Vihollisolion korkeusmitta </param>
    private void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        _vihollinen = new PlatformCharacter(leveys + VihollinenMitta, korkeus + VihollinenMitta);
        
        _vihollinen.Position = paikka;
        _vihollinen.Mass = 4.0;
        _vihollinen.Image = _kuvaTaulukko[0];
        _vihollinen.Tag = "vihollinen";
        
        // Asettaa törmöysehdon.
        AddCollisionHandler(_vihollinen, "nyrkki", NyrkkiTormays);
        
        Add(_vihollinen);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo joulupukin kenttään.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    /// <param name="paikka"> Joulupukin määritetty sijainti kentällä </param>
    /// <param name="leveys"> Joulupukin leveysmitta </param>
    /// <param name="korkeus"> Joulupukin korkeusmitta </param>
    private void LisaaJoulupukki(Vector paikka, double leveys, double korkeus)
    {
        _joulupukki = new PlatformCharacter(leveys + JoulupukkiMitta, korkeus + JoulupukkiMitta);
        
        _joulupukki.Position = paikka;
        _joulupukki.Mass = 4.0;
        _joulupukki.Image = _kuvaTaulukko[3];
        _joulupukki.Tag = "vihollinen";
        
        // Asettaa törmöysehdon.
        AddCollisionHandler(_joulupukki, "nyrkki", NyrkkiTormaysJoulupukkiin);
        
        Add(_joulupukki);
    }
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka lisää pistelaskurin.
    /// </summary>
    private void SaaEnnuste()
    {
        Label otsikko = LuoOtsikko("LUMISADE!!!!!!", "" , 50, Color.Red, 100, true);
        Add(otsikko);
        
        Timer.CreateAndStart(2, () => Remove(otsikko));
        Timer.CreateAndStart(2, LuoLumisade);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo lumihiutaleen.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    private void LuoLumiHiutale()
    {
        
        _lumihiutale = new PhysicsObject(RuudunKoko, RuudunKoko);
        
        _lumihiutale.X = _pelaaja1.X + _satunnainen.Next(-400, 401);
        _lumihiutale.Y = _pelaaja1.Y + _satunnainen.Next(400, 501);
        _lumihiutale.Mass = 4.0;
        _lumihiutale.Image = _lumiHiutaleTaulukko[_satunnainen.Next(0, 5)];
        _lumihiutale.Tag = "vihollinen";
        _lumihiutale.LifetimeLeft = TimeSpan.FromSeconds(1);
        
        // Asettaa törmöysehdon.
        AddCollisionHandler(_lumihiutale, "taso", LumiHiutaleTormays);
        
        Add(_lumihiutale);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo lumisateen.
    /// </summary>
    private void LuoLumisade()
    {
        for (int j = 0; j < _satunnainen.Next(32, 64); j++)
        {
            LuoLumiHiutale();
        }    
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka vastaa lumihiutaleen törmäystapausta seuraavat tapahtumat.
    /// </summary>
    /// <param name="lumihiutale"> Olio, jota tuhotaan. </param>
    /// <param name="taso"> Olio, jonka seurauksena tuhottava olio tuhotaan. </param>
    private void LumiHiutaleTormays(PhysicsObject lumihiutale, PhysicsObject taso)
    {
        // Lumihiutale tuhoutuu törmätessä tasoon.
        lumihiutale.Destroy();
    }
    
    
    /// <summary>
    /// Kategoria: LAHJAT | Aliohjelma, joka luo lahjat kenttään.
    /// </summary>
    /// <param name="paikka"> Lahjaolion määritetty sijainti kentällä. </param>
    /// <param name="leveys"> Lahjaolion leveysmitta. </param>
    /// <param name="korkeus"> Lahjaolion korkeusmitta. </param>
    private void LisaaLahja(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lahja = PhysicsObject.CreateStaticObject(leveys, korkeus);
        lahja.IgnoresCollisionResponse = true;
        lahja.Position = paikka;
        lahja.Image = _kuvaTaulukko[2];
        lahja.Tag = "Lahja";
        
        Add(lahja);
    }
        
    
    /// <summary>
    /// Kategoria: LAHJAT | Aliohjelma, joka vastaa lahjan törmäystapausta seuraavat tapahtumat.
    /// Mitä tapahtuu: kasvattaa pistemäärän, lahja tuhoutuu. 
    /// </summary>
    /// <param name="hahmo"> Olio, jonka seurauksena tuhottava olio tuhotaan. </param>
    /// <param name="lahja"> Olio, jota tuhotaan. </param>
    private void LahjaTormays(PhysicsObject hahmo, PhysicsObject lahja)
    {
        MessageDisplay.Add("Sait Lahjan!");
        _pelaajanPisteet.Value += 1;
        lahja.Destroy();
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka vastaa pisteiden ylityksiä seuraavia tapahtumia.
    /// Myös kutsuu kentänvaihto-funktiota. 
    /// </summary>
    private void RajaYlitetty()
    {
        switch (_pelaajanPisteet.Value)
        {
            case PisteRaja1:
                MessageDisplay.Add("Killing spree!");
                TallennaPisteet();
                break;
            
            case PisteRaja2:
                MessageDisplay.Add("Rampage!");
                TallennaPisteet();
                break;
            
            case PisteRaja3:
                MessageDisplay.Add("Dominating!");
                TallennaPisteet();
                break;
            
            case PisteRaja4:
                MessageDisplay.Add("Unstoppable!");
                TallennaPisteet();
                break;
            
            case PisteRaja5:
                MessageDisplay.Add("Godlike!");
                TallennaPisteet();
                break;
            
            // Lähettää viestin, kasvattaa muuttujan yhdellä ja kutsuu aliohjelma, joka vaihtaa kenttää.
            case PisteRaja6:
                MessageDisplay.Add("Wicked Sick!!");
                TallennaPisteet();
                
                _kenttaNro++;
                SeuraavaKentta();
                break;
        }
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka lisää pistelaskurin.
    /// </summary>
    private void LuoLaskuri()
    {
        _pelaajanPisteet = LuoPisteLaskuri(Screen.Right - 100.0, Screen.Top - 100.0);
        
        _pelaajanPisteet.Value = 0;
        _pelaajanPisteet.MaxValue = 30;
        _pelaajanPisteet.MinValue = 0;
        
        _pelaajanPisteet.AddTrigger(PisteRaja1, TriggerDirection.Up, RajaYlitetty);
        _pelaajanPisteet.AddTrigger(PisteRaja2, TriggerDirection.Up, RajaYlitetty);
        _pelaajanPisteet.AddTrigger(PisteRaja3, TriggerDirection.Up, RajaYlitetty);
        _pelaajanPisteet.AddTrigger(PisteRaja4, TriggerDirection.Up, RajaYlitetty);
        _pelaajanPisteet.AddTrigger(PisteRaja5, TriggerDirection.Up, RajaYlitetty);
        _pelaajanPisteet.AddTrigger(PisteRaja6, TriggerDirection.Up, RajaYlitetty);
        
        // Aktivoi lumisateen toiselle ja viimeiselle pelikentällä, mikä nostattaa vaikeustason.
        if (_kenttaNro == _viimeinenKentta || _kenttaNro == _toinenKentta)
        {
            _pelaajanPisteet.AddTrigger(PisteRaja1, TriggerDirection.Up, SaaEnnuste);
        }
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka luo pistelaskurin.
    /// </summary>
    /// <param name="x"> Pistelaskurin sijainti x-akselilla </param>
    /// <param name="y"> Pistelaskurin sijainti y-akselilla </param>
    /// <returns> Palauttaa laskuriolion. </returns>
    private IntMeter LuoPisteLaskuri(double x, double y)
    {
        
        // Laskuri-olio & toiminallisuus.
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 100;

        // Laskurin grafiikka.
        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.White;
        naytto.BorderColor = Color.Black;
        naytto.Color = Color.Green;
        naytto.Title = "Pisteet: ";
        Add(naytto);
        
        return laskuri;
    }

    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka laskee kokonaispisteet ja näyttää ne voittovalikossa.
    /// </summary>
    private void LaskeKaikkiPisteet()
    {
        int kaikkiPisteet = 0;
        
        // Silmukka laskee taulukon kaikki pisteet yhteen
        for (int i = 0; i < _kokonaisPisteet.Length; i++)
        {
            kaikkiPisteet +=  _kokonaisPisteet[i];
        }
        
        // Muunnetaan string-tietotyyppiseksi.
        string kaikkiPisteetString = $"{kaikkiPisteet}";

        // Luodaan otsikko, joka näyttää pisteiden yhteismäärän.
        Label otsikko = LuoOtsikko("Kokonaispisteesi ovat: ", kaikkiPisteetString, 100, Color.White, 25, false);
        Add(otsikko);
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka tallentaa pisteet taulukkoon.
    /// </summary>
    private void TallennaPisteet()
    {
        _kokonaisPisteet[_kenttaNro - 1] = _pelaajanPisteet.Value;
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka kumoaa pisteet kuoleman jälkeen.
    /// </summary>
    private void KumoaPisteet()
    {
        _kokonaisPisteet[_kenttaNro - 1] = 0;
    }

    /// <summary>
    /// Kategoria: ELEMENTTI | Aliohjelma, joka palauttaa otsikko-elementin johonkin tiettyyn tarpeeseen. Esim. valikkolle.
    /// </summary>
    /// <param name="otsikkoTeksti"> Otsikko-olion varsinainen teksi. </param>
    /// <param name="otsikkoPisteet"> Otsikko-olion osoittama pistemäärä. </param>
    /// <param name="otsikkoY"> Otsikko-olion sijainti y-akselilla. </param>
    /// <param name="otsikkoVari"> Otsikko-olion tekstin väri. </param>
    /// <param name="otsikkoKoko"> Otsikko-olion mitta eli koko. </param>
    /// <param name="otsikkoKorostus"> Otsikko-olion tekstin ominaisuus. </param>
    /// <returns> Palauttaa otsikko olion. </returns>
    private Label LuoOtsikko(string otsikkoTeksti, string otsikkoPisteet, int otsikkoY, Color otsikkoVari, int otsikkoKoko, bool otsikkoKorostus)
    {
        Label otsikko = new Label(otsikkoTeksti + otsikkoPisteet);    
        otsikko.Y = otsikkoY; 
        otsikko.TextColor = otsikkoVari;
        otsikko.Font = new Font(otsikkoKoko, otsikkoKorostus); 
        
        return otsikko;
    }
    
    
    /// <summary>
    ///  Kategoria: ELEMENTTI | Aliohjelma, joka palauttaa räjähdysolion.
    /// </summary>
    /// <param name="olio"> Olio, jonka sijainti otetaan räjähdysefektin sijainniksi. </param>
    /// <returns> Palauttaa räjähdysolion. </returns>
    private Explosion LuoRajahdys(PhysicsObject olio)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = olio.Position;
        
        return rajahdys;
    }
}