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
    
    // Fysiikka-attribuutit.
    private const double Nopeus = 200;
    private const double HyppyNopeus = 750;
    private const int RuudunKoko = 40;
    
    // Kenttätaulukko.
    private static readonly string[] KenttaTaulukko ={"kentta1.txt","kentta2.txt","kentta3.txt"};
    
    // Kuvataulukko. 
    private Image[] _kuvaTaulukko = {LoadImage("ilkeä_ukko.png"),LoadImage("jere_default.png"),LoadImage("lahja.png")};
    
    
    // Olio-attribuutit
    private PlatformCharacter _pelaaja1;
    private PlatformCharacter _vihollinen;
    private PhysicsObject _nyrkki;


    public override void Begin()
    {

        LuoKentta("kentta1.txt");

    }

    
    /// <summary>
    /// Kategoria: PELIKENTTÄ | Aliohejlma, joka luo varsinaisen pelikentän.
    /// </summary>
    /// <param name="seuraavakentta"></param>
    private void LuoKentta(string seuraavakentta)
    {
        // Painovoima & fysiikka
        Gravity = new Vector(0, -1000);
        
        // Pelikentän omaisuudet.
        TileMap kentta = TileMap.FromLevelAsset(seuraavakentta);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaLahja);
        kentta.SetTileMethod('@', LisaaVihollinen);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.Execute(RuudunKoko, RuudunKoko);
        
        // Pelikentän reunat
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkBlue, Color.SkyBlue);
        Surface alareuna = Surface.CreateBottom(Level, 30, 100, 40);
        
        // Kutsut
        Add(alareuna);
        LisaaNappaimet();
        LuoLaskuri();
        
        // Pätkä, joka asettaa zoomauksen pelaajahahmoon.
        Camera.Follow(_pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
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

        Keyboard.Listen(Key.Space, ButtonState.Pressed, LisaaNyrkki, "555", _pelaaja1, -Nopeus);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", _pelaaja1, -Nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", _pelaaja1, Nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", _pelaaja1, HyppyNopeus);
        
        
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
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
        Exit();
        
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