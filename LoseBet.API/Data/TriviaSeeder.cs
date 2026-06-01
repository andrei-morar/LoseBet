using LoseBet.Core.Models;
using System.Collections.Generic;

namespace LoseBet.API.Data
{
    public static class TriviaSeeder
    {
        public static List<TriviaQuestion> GetInitialQuestions()
        {
            var list = new List<TriviaQuestion>();

            // ISTORIE - Grilă
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cine a fost primul domnitor al Țării Românești?", OptionA = "Mircea cel Bătrân", OptionB = "Basarab I", OptionC = "Vlad Țepeș", OptionD = "Mihai Viteazul", CorrectAnswer = "Basarab I" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Ce domnitor a realizat prima unire a Țărilor Române în 1600?", OptionA = "Alexandru Ioan Cuza", OptionB = "Ștefan cel Mare", OptionC = "Mihai Viteazul", OptionD = "Carol I", CorrectAnswer = "Mihai Viteazul" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Din ce dinastie făcea parte Regele Carol I?", OptionA = "Habsburg", OptionB = "Hohenzollern", OptionC = "Romanov", OptionD = "Bourbon", CorrectAnswer = "Hohenzollern" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cine era liderul dacilor în timpul războaielor cu Traian?", OptionA = "Burebista", OptionB = "Deceneu", OptionC = "Decebal", OptionD = "Zamolxis", CorrectAnswer = "Decebal" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cum se numea tratatul care a pus capăt Primului Război Mondial?", OptionA = "Tratatul de la Versailles", OptionB = "Tratatul de la Trianon", OptionC = "Tratatul de la Paris", OptionD = "Tratatul de la Yalta", CorrectAnswer = "Tratatul de la Versailles" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cine a descoperit America în 1492?", OptionA = "Fernando Magellan", OptionB = "Amerigo Vespucci", OptionC = "Cristofor Columb", OptionD = "Vasco da Gama", CorrectAnswer = "Cristofor Columb" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Liderul cărui imperiu a fost Suleiman Magnificul?", OptionA = "Roman", OptionB = "Bizantin", OptionC = "Otoman", OptionD = "Persan", CorrectAnswer = "Otoman" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "În ce oraș a fost asasinat Arhiducele Franz Ferdinand în 1914?", OptionA = "Viena", OptionB = "Sarajevo", OptionC = "Belgrad", OptionD = "Budapesta", CorrectAnswer = "Sarajevo" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cine a fost liderul URSS în timpul celui de-al Doilea Război Mondial?", OptionA = "Vladimir Lenin", OptionB = "Iosif Stalin", OptionC = "Nikita Hrușciov", OptionD = "Mihail Gorbaciov", CorrectAnswer = "Iosif Stalin" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Care a fost cel mai lung război din istorie?", OptionA = "Războiul de 100 de ani", OptionB = "Războiul de 30 de ani", OptionC = "Războiul Rece", OptionD = "Războiul Rozelor", CorrectAnswer = "Războiul de 100 de ani" });

            // ISTORIE - Aproximare
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a avut loc Marea Unire de la Alba Iulia?", CorrectAnswer = "1918" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a căzut regimul comunist în România?", CorrectAnswer = "1989" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a început Primul Război Mondial?", CorrectAnswer = "1914" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a urcat pe tron Ștefan cel Mare?", CorrectAnswer = "1457" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "Câți ani a durat Războiul de 100 de ani?", CorrectAnswer = "116" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a devenit România regat?", CorrectAnswer = "1881" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "Câte state au semnat inițial Declarația de Independență a SUA?", CorrectAnswer = "13" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a început al Doilea Război Mondial?", CorrectAnswer = "1939" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a fost asasinat președintele american J.F. Kennedy?", CorrectAnswer = "1963" });
            list.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an s-a scufundat Titanicul?", CorrectAnswer = "1912" });

            // GEOGRAFIE - Grilă
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este capitala Australiei?", OptionA = "Sydney", OptionB = "Melbourne", OptionC = "Canberra", OptionD = "Perth", CorrectAnswer = "Canberra" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este cel mai înalt vârf muntos din România?", OptionA = "Omu", OptionB = "Negoiu", OptionC = "Moldoveanu", OptionD = "Peleaga", CorrectAnswer = "Moldoveanu" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "În ce mare se varsă fluviul Dunărea?", OptionA = "Marea Roșie", OptionB = "Marea Neagră", OptionC = "Marea Mediterană", OptionD = "Marea Adriatică", CorrectAnswer = "Marea Neagră" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Pe ce continent se află Deșertul Sahara?", OptionA = "Asia", OptionB = "Africa", OptionC = "America de Sud", OptionD = "Australia", CorrectAnswer = "Africa" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este cel mai mic stat din lume?", OptionA = "Monaco", OptionB = "Vatican", OptionC = "San Marino", OptionD = "Liechtenstein", CorrectAnswer = "Vatican" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este capitala Canadei?", OptionA = "Toronto", OptionB = "Vancouver", OptionC = "Ottawa", OptionD = "Montreal", CorrectAnswer = "Ottawa" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "În ce țară se află Turnul din Pisa?", OptionA = "Franța", OptionB = "Spania", OptionC = "Italia", OptionD = "Grecia", CorrectAnswer = "Italia" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este cel mai mare ocean al planetei?", OptionA = "Atlantic", OptionB = "Indian", OptionC = "Arctic", OptionD = "Pacific", CorrectAnswer = "Pacific" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Ce râu traversează municipiul Cluj-Napoca?", OptionA = "Mureș", OptionB = "Olt", OptionC = "Someșul Mic", OptionD = "Crișul Repede", CorrectAnswer = "Someșul Mic" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este cel mai mare județ din România ca suprafață?", OptionA = "Suceava", OptionB = "Timiș", OptionC = "Caraș-Severin", OptionD = "Constanța", CorrectAnswer = "Timiș" });

            // GEOGRAFIE - Aproximare
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Care este altitudinea vârfului Moldoveanu (în metri)?", CorrectAnswer = "2544" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte județe are România (fără București)?", CorrectAnswer = "41" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Ce lungime are fluviul Dunărea pe teritoriul României (în km)?", CorrectAnswer = "1075" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte continente există pe Glob?", CorrectAnswer = "7" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Care este altitudinea Vârfului Everest (în metri)?", CorrectAnswer = "8848" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte oceane sunt pe Pământ?", CorrectAnswer = "5" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte state compun Statele Unite ale Americii?", CorrectAnswer = "50" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte țări împart graniță terestră cu România?", CorrectAnswer = "5" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte fusuri orare are Rusia?", CorrectAnswer = "11" });
            list.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Care este adâncimea maximă a Gropii Marianelor (în metri)?", CorrectAnswer = "10994" });

            // SPORT - Grilă
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Cine a câștigat Cupa Mondială la fotbal în 2022?", OptionA = "Franța", OptionB = "Brazilia", OptionC = "Argentina", OptionD = "Germania", CorrectAnswer = "Argentina" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "În ce sport excelează Simona Halep?", OptionA = "Gimnastică", OptionB = "Tenis", OptionC = "Înot", OptionD = "Atletism", CorrectAnswer = "Tenis" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Care echipă românească a câștigat Cupa Campionilor Europeni la fotbal?", OptionA = "Dinamo", OptionB = "Universitatea Craiova", OptionC = "Steaua", OptionD = "CFR Cluj", CorrectAnswer = "Steaua" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Cine este considerat \"Zeul\" baschetului, având numărul 23 la Chicago Bulls?", OptionA = "LeBron James", OptionB = "Kobe Bryant", OptionC = "Shaquille O'Neal", OptionD = "Michael Jordan", CorrectAnswer = "Michael Jordan" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Ce sportivă a luat prima nota 10 din istoria gimnasticii?", OptionA = "Cătălina Ponor", OptionB = "Nadia Comăneci", OptionC = "Andreea Răducan", OptionD = "Sandra Izbașa", CorrectAnswer = "Nadia Comăneci" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Cine deține recordul mondial la suta de metri sprint?", OptionA = "Tyson Gay", OptionB = "Yohan Blake", OptionC = "Usain Bolt", OptionD = "Carl Lewis", CorrectAnswer = "Usain Bolt" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Cum se numește cel mai important turneu de tenis pe iarbă?", OptionA = "Roland Garros", OptionB = "US Open", OptionC = "Wimbledon", OptionD = "Australian Open", CorrectAnswer = "Wimbledon" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Câte reprize (sferturi) are un meci de baschet?", OptionA = "2", OptionB = "3", OptionC = "4", OptionD = "5", CorrectAnswer = "4" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "Cine a câștigat Balonul de Aur de cele mai multe ori?", OptionA = "C. Ronaldo", OptionB = "Lionel Messi", OptionC = "Pele", OptionD = "Maradona", CorrectAnswer = "Lionel Messi" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 2, Text = "În ce oraș joacă echipa de fotbal Real Madrid?", OptionA = "Barcelona", OptionB = "Valencia", OptionC = "Sevilla", OptionD = "Madrid", CorrectAnswer = "Madrid" });

            // SPORT - Aproximare
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "În ce an a apărat Helmuth Duckadam 4 penalty-uri la Sevilla?", CorrectAnswer = "1986" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "În ce an a primit Nadia Comăneci primul 10 din istoria gimnasticii?", CorrectAnswer = "1976" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte goluri a marcat Gică Hagi pentru Naționala României?", CorrectAnswer = "35" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte minute durează un meci de fotbal (fără prelungiri)?", CorrectAnswer = "90" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte titluri de Grand Slam a câștigat Rafael Nadal?", CorrectAnswer = "22" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câți jucători are o echipă de volei pe teren?", CorrectAnswer = "6" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte puncte valorează o aruncare de la distanță (din afara semicercului) în baschet?", CorrectAnswer = "3" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte medalii de aur olimpice a câștigat înotătorul Michael Phelps?", CorrectAnswer = "23" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "La ce vârstă s-a retras Usain Bolt din atletism?", CorrectAnswer = "31" });
            list.Add(new TriviaQuestion { Category = "Sport", Type = 1, Text = "Câte secunde durează o rundă de box la profesioniști?", CorrectAnswer = "180" });

            // ȘTIINȚE - Grilă
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Care este formula chimică a apei?", OptionA = "CO2", OptionB = "H2O", OptionC = "NaCl", OptionD = "O2", CorrectAnswer = "H2O" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Care este cea mai apropiată planetă de Soare?", OptionA = "Venus", OptionB = "Pământ", OptionC = "Marte", OptionD = "Mercur", CorrectAnswer = "Mercur" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Cine a formulat Teoria Relativității?", OptionA = "Isaac Newton", OptionB = "Nikola Tesla", OptionC = "Albert Einstein", OptionD = "Galileo Galilei", CorrectAnswer = "Albert Einstein" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Ce gaz consumă plantele pentru a realiza fotosinteza?", OptionA = "Oxigen", OptionB = "Hidrogen", OptionC = "Dioxid de carbon", OptionD = "Azot", CorrectAnswer = "Dioxid de carbon" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Cine a descoperit penicilina?", OptionA = "Alexander Fleming", OptionB = "Louis Pasteur", OptionC = "Marie Curie", OptionD = "Robert Koch", CorrectAnswer = "Alexander Fleming" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Care este cel mai dur material natural de pe Pământ?", OptionA = "Aurul", OptionB = "Fierul", OptionC = "Diamantul", OptionD = "Cuarțul", CorrectAnswer = "Diamantul" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Ce forță ne ține cu picioarele pe Pământ?", OptionA = "Forța electromagnetică", OptionB = "Gravitația", OptionC = "Frecarea", OptionD = "Inerția", CorrectAnswer = "Gravitația" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Care este centrul de control al celulei umane?", OptionA = "Mitocondria", OptionB = "Citoplasma", OptionC = "Membrana", OptionD = "Nucleul", CorrectAnswer = "Nucleul" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Ce element chimic are simbolul \"Fe\"?", OptionA = "Fluor", OptionB = "Fosfor", OptionC = "Fier", OptionD = "Franciu", CorrectAnswer = "Fier" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 2, Text = "Care este cel mai mare organ al corpului uman?", OptionA = "Inima", OptionB = "Ficatul", OptionC = "Pielea", OptionD = "Plămânii", CorrectAnswer = "Pielea" });

            // ȘTIINȚE - Aproximare
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câte oase are corpul uman adult?", CorrectAnswer = "206" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Care este viteza luminii în vid (în km/s)?", CorrectAnswer = "300000" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "La câte grade Celsius fierbe apa (la nivelul mării)?", CorrectAnswer = "100" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câte planete sunt în Sistem Solar?", CorrectAnswer = "8" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câți dinți are un om adult complet (inclusiv măselele de minte)?", CorrectAnswer = "32" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "La câte grade Celsius îngheață apa?", CorrectAnswer = "0" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câți cromozomi are o celulă umană normală?", CorrectAnswer = "46" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Ce procent din suprafața Pământului este acoperit de apă?", CorrectAnswer = "71" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câte zile durează rotația completă a Pământului în jurul Soarelui (fără an bisect)?", CorrectAnswer = "365" });
            list.Add(new TriviaQuestion { Category = "Științe", Type = 1, Text = "Câte elemente chimice se găsesc oficial în tabelul periodic?", CorrectAnswer = "118" });

            // ARTĂ, DIVERSE etc (omitem detaliile pentru brevitate, pot fi adăugate la cerere)

            return list;
        }
    }
}