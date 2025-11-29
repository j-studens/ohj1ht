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
    
    // Fysiikka-attribuutit.
    private const double Nopeus = 200;
    private const double HyppyNopeus = 750;
    private const int RuudunKoko = 40;
    
    // Kenttätaulukko.
    private static readonly string[] KenttaTaulukko ={"kentta1.txt","kentta2.txt","kentta3.txt"};
    
    // Kuvataulukko. 
    private Image[] _kuvaTaulukko = {LoadImage("ilkeä_ukko.png"),LoadImage("jere_default.png"),LoadImage("lahja.png"),LoadImage("joulupukki.png")};
    
    // Valikkotaulukko
    private string[] _alkuvaihtoehdot = { "Aloita peli", "Lopeta" };
    private string[] _toisetvaihtoehdot = { "Jatka peli", "Lopeta" };
    
    // Olio-attribuutit
    private PlatformCharacter _pelaaja1;
    private PlatformCharacter _vihollinen;
    private PlatformCharacter _joulupukki;
    private PhysicsObject _nyrkki;

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
        
        // Toteuttaa muulloin kuin pelaajan kuoleman seurauksena.
        if (!_aiheuttikoKuolema)
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
        
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("", vaihtoehdot);
        
        // Toteuttaa, jos aliohjelma kutsuttiin pelaajan kuoleman seurauksena.
        if (_aiheuttikoKuolema)
        {
            valikkoY = -125;
            valikkoVäri = Color.Red;
        }
        
        alkuvalikko.X = 0;
        alkuvalikko.Y = valikkoY;
        alkuvalikko.Color = valikkoVäri;
        alkuvalikko.CapturesMouse = false;
        alkuvalikko.AddItemHandler(0, KenttaSeuraava);
        alkuvalikko.AddItemHandler(1, Exit);
        Add(alkuvalikko);
        
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
    /// Kategoria: PELIKENTTÄ | Aliohejlma, joka luo varsinaisen pelikentän.
    /// </summary>
    /// <param name="seuraavakentta"></param>
    private void LuoKentta(string seuraavakentta)
    {
        // Painovoima & fysiikka
        Gravity = new Vector(0, -1000);
        
        // Asettaa muuttujan lepotilaan, jotta valikkojen ulkonäkö pysyisi järkevänä
        _aiheuttikoKuolema = false;
            
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
                Exit();
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
        _nyrkki = new PlatformCharacter(60, 60);
        _nyrkki.X = hahmo.X + 20;
        _nyrkki.Y = hahmo.Y;
        _nyrkki.Image = _kuvaTaulukko[1];
        _nyrkki.Tag = "nyrkki";
        _nyrkki.LifetimeLeft = TimeSpan.FromSeconds( 0.05 );
        Add(_nyrkki);
    }
    
    /// <summary>
    /// Kategoria: OHJAIMET | Aliohjelma, joak käsittelee nyrkkitörmäystapausta.
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
        _vihollinen = new PlatformCharacter(leveys, korkeus);
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
        _joulupukki = new PlatformCharacter(leveys, korkeus);
        _joulupukki.Position = paikka;
        _joulupukki.Mass = 4.0;
        _joulupukki.Image = _kuvaTaulukko[3];
        
        Add(_joulupukki);
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