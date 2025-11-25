using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author J
/// @version 19.11.2025
/// <summary>
/// 
/// </summary>
public class ohj1ht_peli : PhysicsGame
{
    int kenttaNro = 1;
    private const int pisteraja1 = 4;
    private const int pisteraja2 = 7;
    private const int pisteraja3 = 9;
    private const int pisteraja4 = 11;
    private const int pisteraja5 = 13;
    private const int pisteraja6 = 15;
    
    
    IntMeter pelaajanPisteet;
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private Image VihollinenKuva = LoadImage("ilkeä_ukko.png");
    private Image pelaajanKuva = LoadImage("jere_default.png");
    private Image tahtiKuva = LoadImage("lahja.png");
    private PlatformCharacter pelaaja1;
    private PlatformCharacter vihollinen;
    private PhysicsObject nyrkki;


    public override void Begin()
    {

        LuoKentta("kentta1.txt");

    }

    void LuoKentta(string seuraavakentta)
    {
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset(seuraavakentta);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisääLahja);
        kentta.SetTileMethod('@', LisaaVihollinen);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkBlue, Color.SkyBlue);
        Surface alareuna = Surface.CreateBottom(Level, 30, 100, 40);
        Add(alareuna);
        LisaaNappaimet();
        LuoLaskuri();
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
    }



     void LisaaNappaimet()
    {

        Keyboard.Listen(Key.Space, ButtonState.Pressed, LisaaNyrkki, "555", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        
        
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
    
    void LisaaNyrkki(PlatformCharacter hahmo, double nopeus)
    {
        nyrkki = new PlatformCharacter(60, 60);
        nyrkki.X = hahmo.X + 20;
        nyrkki.Y = hahmo.Y;
        nyrkki.Image = pelaajanKuva;
        nyrkki.Tag = "nyrkki";
        nyrkki.LifetimeLeft = TimeSpan.FromSeconds( 0.05 );
        Add(nyrkki);
    }

    
    void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.White;
        Add(taso);
    }
    /// <summary>
    /// Aliohjelma, joka luo pelaajahahmon
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "Lahja", LahjaTörmäys);
        AddCollisionHandler(pelaaja1, "vihollinen", VihollinenTörmäys);
        Add(pelaaja1);
    }
    
    
    void VihollinenTörmäys(PhysicsObject pelaaja, PhysicsObject vihollinen)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = pelaaja.Position;
        Add(rajahdys);
        pelaaja.Destroy();
        
        
    }
    
    /// <summary>
    /// Aliohjelma, joak käsittelee nyrkkitörmäystapausta
    /// </summary>
    /// <param name="vihollinen"></param>
    /// <param name="nyrkki"></param>
    void NyrkkiTörmäys(PhysicsObject vihollinen, PhysicsObject nyrkki)
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = vihollinen.Position;
        Add(rajahdys);
        vihollinen.Destroy();
        pelaajanPisteet.Value += 1;
        
    }
    
    void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        vihollinen = new PlatformCharacter(leveys, korkeus);
        vihollinen.Position = paikka;
        vihollinen.Mass = 4.0;
        vihollinen.Image = VihollinenKuva;
        vihollinen.Tag = "vihollinen";
        AddCollisionHandler(vihollinen, "nyrkki", NyrkkiTörmäys);
        Add(vihollinen);
    }

    
    /// LAHJAT
    /// <summary>
    /// Aliohjelma, joka luo lahjat kenttään
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    void LisääLahja(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject Lahja = PhysicsObject.CreateStaticObject(leveys, korkeus);
        Lahja.IgnoresCollisionResponse = true;
        Lahja.Position = paikka;
        Lahja.Image = tahtiKuva;
        Lahja.Tag = "Lahja";
        Add(Lahja);
    }
        
    /// LAHJAT
    /// <summary>
    /// Aliohjelma, joka määrittä sen mitä tapahtuu törmäystapauksessa. Samalla lisää pisteen 
    /// </summary>
    /// <param name="hahmo"></param>
    /// <param name="tahti"></param>
    void LahjaTörmäys(PhysicsObject hahmo, PhysicsObject tahti)
    {
        MessageDisplay.Add("Sait Lahjan!");
        tahti.Destroy();
        pelaajanPisteet.Value += 1;
    }
    
    void RajaYlitetty()
    {
        switch (pelaajanPisteet.Value)
        {
            case pisteraja1:
                MessageDisplay.Add("Killing spree!");
                break;
            case pisteraja2:
                MessageDisplay.Add("Rampage!");
                break;
            case pisteraja3:
                MessageDisplay.Add("Dominating!");
                break;
            case pisteraja4:
                MessageDisplay.Add("Unstoppable!");
                break;
            case pisteraja5:
                MessageDisplay.Add("Godlike!");
                break;
            case pisteraja6:
                MessageDisplay.Add("Wicked Sick!!");
                kenttaNro++;
                SeuraavaKentta();
                break;
        }
    }
    
    /// PISTEET
    /// <summary>
    /// Aliohjelma, joka lisää pistelaskurin
    /// </summary>
    void LuoLaskuri()
    {
        pelaajanPisteet = LuoPisteLaskuri(Screen.Right - 100.0, Screen.Top - 100.0);
        pelaajanPisteet.Value = 0;
        pelaajanPisteet.MaxValue = 30;
        pelaajanPisteet.MinValue = 0;
        pelaajanPisteet.AddTrigger(pisteraja1, TriggerDirection.Up, RajaYlitetty);
        pelaajanPisteet.AddTrigger(pisteraja2, TriggerDirection.Up, RajaYlitetty);
        pelaajanPisteet.AddTrigger(pisteraja3, TriggerDirection.Up, RajaYlitetty);
        pelaajanPisteet.AddTrigger(pisteraja4, TriggerDirection.Up, RajaYlitetty);
        pelaajanPisteet.AddTrigger(pisteraja5, TriggerDirection.Up, RajaYlitetty);
        pelaajanPisteet.AddTrigger(pisteraja6, TriggerDirection.Up, RajaYlitetty);
    }
    
    /// PISTEET
    /// <summary>
    /// Aliohjelma, joka luo pistelaskurin
    /// </summary>
    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 100;

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
    
    void SeuraavaKentta()
    {
        ClearAll();
        
        switch (kenttaNro)
        {
            case 1:
                LuoKentta("kentta1.txt");
                break;
            case 2:
                LuoKentta("kentta2.txt");
                break;
            case 3:
                LuoKentta("kentta3.txt");
                break;
            case 4:
                Exit();
                break;
        }
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