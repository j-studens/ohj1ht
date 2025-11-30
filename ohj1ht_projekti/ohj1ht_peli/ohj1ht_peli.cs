using System;
using Jypeli;
using Jypeli.Assets;

/// @author J
/// @version 19.11.2025
/// <summary>
/// 
/// </summary>
public class ohj1ht_peli : PhysicsGame
{
    
    // Piste-attribuutit.    
    private const int PisteRaja1 = 4;
    private const int PisteRaja2 = 7;
    private const int PisteRaja3 = 9;
    private const int PisteRaja4 = 11;
    private const int PisteRaja5 = 13;
    private const int PisteRaja6 = 15;
    private IntMeter _pelaajanPisteet;
    
    // Kenttä attribuutti.
    private int _kenntaNro = 1;
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
    
    // Kuvataulukko. 
    private Image[] _kuvaTaulukko = {LoadImage("ilkeä_ukko.png"),LoadImage("jere_default.png"),LoadImage("lahja.png"),LoadImage("joulupukki.png"),LoadImage("lumihiutale.png")};
    private Image[] _nyrkkeilyTaulukko = {LoadImage("vasen_nyrkki.png"),LoadImage("oikea_nyrkki.png"),LoadImage("jere_ilkeä.png")};
    
    // Valikkotaulukko
    private string[] _alkuvaihtoehdot = { "Aloita peli", "Lopeta" };
    private string[] _toisetvaihtoehdot = { "Jatka peli", "Lopeta" };
    private string[] _uudetvaihtoehdot = { "Uusi peli", "Lopeta" };
    
    // Olio-attribuutit
    private PlatformCharacter _pelaaja1;
    private PlatformCharacter _vihollinen;
    private PlatformCharacter _joulupukki;
    private PhysicsObject _nyrkki1;
    private PhysicsObject _nyrkki2;
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
    private void LisaaValikko(string[] vaihtoehdot)
    {
        
        int valikkoY = 0;
        Color valikkoVäri = Color.DarkGreen;
        
        // Toteuttaa muulloin kuin pelaajan kuoleman tai voiton seurauksena.
        if (!_aiheuttikoKuolema && !_aiheuttikoVoitto)
        {
            ClearAll();
            Level.Background.CreateGradient(Color.LightBlue, Color.DarkBlue);   
            
            // Luodaan otsikko
            Label otsikko = new Label("LUMIUKKO FIGHTER"); 
            otsikko.Y = 180; 
            otsikko.Font = new Font(100, true);
            otsikko.TextColor = Color.White;
            Add(otsikko);
        }
        
        
        // Toteuttaa, jos aliohjelma kutsuttiin pelaajan kuoleman seurauksena.
        if (_aiheuttikoKuolema)
        {
            valikkoY = -125;
            valikkoVäri = Color.Red;
        }
        
        // Toteuttaa, jos aliohjelma kutsuttiin voiton seurauksena.
        if (_aiheuttikoVoitto)
        {
            valikkoY = -125;
            valikkoVäri = Color.Green;
            vaihtoehdot = _uudetvaihtoehdot;
        }
        
        MultiSelectWindow valikko = new MultiSelectWindow("", vaihtoehdot);
        valikko.X = 0;
        valikko.Y = valikkoY;
        valikko.Color = valikkoVäri;
        valikko.CapturesMouse = false;
        valikko.AddItemHandler(0, KenttaSeuraava);
        valikko.AddItemHandler(1, Exit);
        Add(valikko);
        
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
    /// Kategoria: VALIKKO | Aliohjelma, joka käsittelee kuolematapausta. Luo erilaisen valikon
    /// </summary>
    private void OletKuollut()
    {
        // Poistetaan kaikki ja asetetaan uusi taustaväri
        ClearAll();
        Level.Background.CreateGradient(Color.Red, Color.DarkRed);  
        
        // Luodaan otsikko
        Label otsikko = new Label("KUOLIT"); 
        otsikko.Y = 50; 
        otsikko.Font = new Font(100, true); 
        Add(otsikko);
        
        _aiheuttikoKuolema = true;
        
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
        Label otsikko = new Label("VOITTO!"); 
        otsikko.Y = 50; 
        otsikko.Font = new Font(100, true); 
        otsikko.TextColor = Color.White;
        Add(otsikko);
        
        _aiheuttikoVoitto = true;
        
        // Kutsutaan valikko ja resetoidaan kenttänumero.
        _kenntaNro = 1;
        Timer.CreateAndStart(0.5, ValikkoSeuraava);
    }
    
    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohejlma, joka luo varsinaisen pelikentän.
    /// </summary>
    /// <param name="seuraavakentta"></param>
    private void LuoKentta(string seuraavakentta)
    {
        // Painovoima & fysiikka
        Gravity = new Vector(0, -1000);
        
        // Asettaa muuttujan lepotilaan, jotta valikkojen ulkonäkö pysyisi järkevänä
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
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
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
        ClearAll();
        
        switch (_kenntaNro)
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
    /// <param name="hahmo"></param>
    /// <param name="nopeus"></param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka käsittelee hyppyjen toiminnallisuutta. Kutsuttava.
    /// </summary>
    /// <param name="hahmo"></param>
    /// <param name="nopeus"></param>
    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joka luo nyrkkiolion, jolla on lyhyt elinikä. Kutsuttava.
    /// </summary>
    /// <param name="hahmo"></param>
    /// <param name="nopeus"></param>
    private void LisaaNyrkki(PlatformCharacter hahmo, double nopeus)
    {
        // Oikea nyrkki.
        _nyrkki1 = new PlatformCharacter(60, 60);
        _nyrkki1.X = hahmo.X - 20;
        _nyrkki1.Y = hahmo.Y;
        _nyrkki1.Image = _nyrkkeilyTaulukko[1];
        _nyrkki1.Tag = "nyrkki";
        _nyrkki1.LifetimeLeft = TimeSpan.FromSeconds( 0.05 );
        
        // Vasen nyrkki.
        _nyrkki2 = new PlatformCharacter(60, 60);
        _nyrkki2.X = hahmo.X + 20;
        _nyrkki2.Y = hahmo.Y;
        _nyrkki2.Image = _nyrkkeilyTaulukko[0];
        _nyrkki2.Tag = "nyrkki";
        _nyrkki2.LifetimeLeft = TimeSpan.FromSeconds( 0.05 );
        
        // Pelaajahahmon ilme muuttuu
        _pelaaja1.Image = _nyrkkeilyTaulukko[2];
        
        Add(_nyrkki1);
        Add(_nyrkki2);
        
        Timer.CreateAndStart(1, JerenIlme);
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
    /// Mitä tapahtuu: kasvattaa pistemäärän, räjähdys, vihollinen kuolee.
    /// </summary>
    /// <param name="vihollinen"></param>
    /// <param name="nyrkki"></param>
    private void NyrkkiTörmäys(PhysicsObject vihollinen, PhysicsObject nyrkki)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = vihollinen.Position;
        _pelaajanPisteet.Value += 1;
        Add(rajahdys);
        vihollinen.Destroy();
    }
    
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joak käsittelee nyrkkitörmäystapausta joulupukin suhteen.
    /// Mitä tapahtuu: voittotapahtuma ja ilmestyy voittovalikko.
    /// </summary>
    /// <param name="joulupukki"></param>
    /// <param name="nyrkki"></param>
    private void NyrkkiTörmäysJoulupukkiin(PhysicsObject joulupukki, PhysicsObject nyrkki)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = joulupukki.Position;
        _pelaajanPisteet.Value += 1;
        Add(rajahdys);
        joulupukki.Destroy();
        
        //Voitto
        Timer.CreateAndStart(0.3, OletVoittanut);
    }
    
    
    /// <summary>
    /// Kategoria: PELAAJA | Aliohjelma, joka luo pelaajahahmon.
    /// Samalla luo törmäysehdot
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        _pelaaja1 = new PlatformCharacter(leveys, korkeus);
        _pelaaja1.Position = paikka;
        _pelaaja1.Mass = 4.0;
        _pelaaja1.Image = _kuvaTaulukko[1];
        
        
        // Törmäysehdot: vihollinen & lahja
        AddCollisionHandler(_pelaaja1, "Lahja", LahjaTörmäys);
        AddCollisionHandler(_pelaaja1, "vihollinen", VihollinenTörmäys);
        
        
        Add(_pelaaja1);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka vastaa vihollisen törmäystapausta seuraavat tapahtumat.
    /// Mitä tapahtuu: räjähdys, pelaajahahmo kuolee, poistutaan pelistä.
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="vihollinen"></param>
    private void VihollinenTörmäys(PhysicsObject pelaaja, PhysicsObject vihollinen)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = pelaaja.Position;
        
        Add(rajahdys);
        pelaaja.Destroy();
        Timer.CreateAndStart(0.3, OletKuollut);
        
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo vihollisen kenttään.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        _vihollinen = new PlatformCharacter(leveys + VihollinenMitta, korkeus + VihollinenMitta);
        _vihollinen.Position = paikka;
        _vihollinen.Mass = 4.0;
        _vihollinen.Image = _kuvaTaulukko[0];
        _vihollinen.Tag = "vihollinen";
        
        AddCollisionHandler(_vihollinen, "nyrkki", NyrkkiTörmäys);
        Add(_vihollinen);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo joulupukin kenttään.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void LisaaJoulupukki(Vector paikka, double leveys, double korkeus)
    {
        _joulupukki = new PlatformCharacter(leveys + JoulupukkiMitta, korkeus + JoulupukkiMitta);
        _joulupukki.Position = paikka;
        _joulupukki.Mass = 4.0;
        _joulupukki.Image = _kuvaTaulukko[3];
        _joulupukki.Tag = "vihollinen";
        
        AddCollisionHandler(_joulupukki, "nyrkki", NyrkkiTörmäysJoulupukkiin);
        Add(_joulupukki);
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka lisää pistelaskurin.
    /// </summary>
    private void SaaEnnuste()
    {
        Label otsikko = new Label("LUMISADE!!!!!!"); 
        
        otsikko.Y = 50; 
        otsikko.Font = new Font(100, true); 
        otsikko.TextColor = Color.Red;
        Add(otsikko);
        
        Timer.CreateAndStart(2, otsikko.Destroy);
        Timer.CreateAndStart(2, LuoLumisade);
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    private void LuoLumisade()
    {
        for (int j = 0; j < _satunnainen.Next(32, 64); j++)
        {
            LuoLumiHiutale();
        }    
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka luo lumihiutaleen.
    /// Samalla asettaa viholliselle törmäysehdon.
    /// </summary>
    private void LuoLumiHiutale()
    {
        
        _lumihiutale = new PhysicsObject(RuudunKoko, RuudunKoko);
        
        _lumihiutale.X = _pelaaja1.X + _satunnainen.Next(-300, 301);
        _lumihiutale.Y = _pelaaja1.Y + _satunnainen.Next(300, 401);
        _lumihiutale.Mass = 4.0;
        _lumihiutale.Image = _kuvaTaulukko[4];
        _lumihiutale.Tag = "vihollinen";
        _lumihiutale.LifetimeLeft = TimeSpan.FromSeconds(0.8);
        
        AddCollisionHandler(_lumihiutale, "taso", LumiHiutaleTormays);
        Add(_lumihiutale);
    }
    
    
    /// <summary>
    /// Kategoria: VIHOLLINEN | Aliohjelma, joka vastaa lumihiutaleen törmäystapausta seuraavat tapahtumat.
    /// </summary>
    private void LumiHiutaleTormays(PhysicsObject lumihiutale, PhysicsObject taso)
    {
        lumihiutale.Destroy();
    }
    
    
    /// <summary>
    /// Kategoria: LAHJAT | Aliohjelma, joka luo lahjat kenttään.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
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
    /// <param name="hahmo"></param>
    /// <param name="tahti"></param>
    private void LahjaTörmäys(PhysicsObject hahmo, PhysicsObject tahti)
    {
        MessageDisplay.Add("Sait Lahjan!");
        _pelaajanPisteet.Value += 1;
        tahti.Destroy();
        
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
                break;
            
            case PisteRaja2:
                MessageDisplay.Add("Rampage!");
                break;
            
            case PisteRaja3:
                MessageDisplay.Add("Dominating!");
                break;
            
            case PisteRaja4:
                MessageDisplay.Add("Unstoppable!");
                break;
            
            case PisteRaja5:
                MessageDisplay.Add("Godlike!");
                break;
            
            // Lähettää viestin, kasvattaa muuttujan yhdellä ja kutsuu aliohjelma, joka vaihtaa kenttää.
            case PisteRaja6:
                MessageDisplay.Add("Wicked Sick!!");
                _kenntaNro++;
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
        
        // Viimeinen pelikenttä
        if (_kenntaNro == 3)
        {
            _pelaajanPisteet.AddTrigger(PisteRaja1, TriggerDirection.Up, SaaEnnuste);
        }
    }
    
    
    /// <summary>
    /// Kategoria: PISTEET | Aliohjelma, joka luo pistelaskurin.
    /// </summary>
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
    
    
}
   
//ylhäältä while loop 
//WHILE --- CREATE INFINITE EVIL SNOWFLAKE BOMBS
// NO COLLISION WITH ENEMY PLAYER AND PLATFORMS
// GRAVITY BOUND
// REMOVE WHEN AT THE BOTTOM
// DESTROY EXPLODE WHEN HIT PLAYER
// GENERATE AT RANDOM X AXIS, ALWAYS GENERATE AT SAME Y AXIS
// RANDOM IS A FUNKTION ( DONT INVENT WHEEL AGAIN)
// SNOWFALL 