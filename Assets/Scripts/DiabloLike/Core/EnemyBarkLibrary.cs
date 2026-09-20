using UnityEngine;

namespace DiabloLike.Core
{
    public enum EnemyBarkEvent
    {
        Spawn,
        Searching,
        Damaged,
        HitPlayer,
        Dying
    }

    public static class EnemyBarkLibrary
    {
        private static readonly string[] Spawn =
        {
            "Tak koho dneska zkazime?",
            "Vonis jako spatne rozhodnuti.",
            "No nazdar, dalsi hrdina.",
            "Tohle bude kratky a trapny.",
            "Prisels umrit stylove?",
            "Kruh si objednal maso.",
            "Mam hlad a blbou naladu.",
            "Konecne neco k okusovani.",
            "Hrdina? Spis chodici loot.",
            "Tak se ukaz, legendo.",
            "Udelame z tebe pouceni.",
            "Dalsi den, dalsi naivka.",
            "Tvoje odvaha smrdi potem.",
            "Tohle bude bolet hlavne tebe.",
            "Pekny mec. Skoda ruky.",
            "Vitej v blbem napadu.",
            "Kruh rikal: prines veceri.",
            "Zase nekdo bez planku.",
            "Uzasne, maso prislo samo.",
            "Tvoje nadeje je roztomila.",
            "Dneska mam cynismus navic.",
            "Nekric, jeste jsme nezacali.",
            "Kdo te sem pustil, zoufalce?",
            "Tma ma dneska dobrou naladu.",
            "Jdu ti prepsat epitaf."
        };

        private static readonly string[] Searching =
        {
            "Kam ses schoval, hrdino?",
            "Citit te je lehci nez myslet.",
            "Pojd ven, neboj se trapasu.",
            "Tvoje kroky lzou fakt mizerne.",
            "Hledam odvahu, nasel jsem pach.",
            "Kdepak jsi, mala tragedie?",
            "Neboj, smrt ma trpelivost.",
            "Ticho. To znamena pruser.",
            "Utikas jak dluh po vyplate.",
            "Tak kde je ten slavny spasitel?",
            "Vidim stopy a spatne volby.",
            "Schovavani ti jde lip nez boj.",
            "Tvoje strategie je panika?",
            "Citim pot a levne kouzlo.",
            "Pojd bliz, chci byt zklamany.",
            "Kdyz te najdu, bude to trapne.",
            "Ten strach ma pekny ocas.",
            "Dneska delas turistu v pekle?",
            "Hledam hrace, nachazim problem.",
            "No tak, nechci behat zbytecne.",
            "Kdyby blbost svitila, zaras.",
            "Tvoje srdce mlati jak buben.",
            "Vylez, nebo ti dam recenzi.",
            "Kruh se nudi. Ja taky.",
            "Pach statecnosti nikde."
        };

        private static readonly string[] Damaged =
        {
            "Au. To bylo osobni, co?",
            "Tak ty umis mavat klackem.",
            "Hezky. Skoro jsem respektoval.",
            "Sakra, tohle jsem citil.",
            "Ty malej protivnej problem.",
            "No vyborne, uz krvacim stylove.",
            "Tohle ti sectu, hrdino.",
            "Dobre, ted jsem nastvany.",
            "Mec mas ostrejsi nez vtipy.",
            "Au, moje hnusna duse.",
            "Tak jo, bez rukavic.",
            "Kdo te naucil mirit?",
            "Zasah. Nezvykni si.",
            "To byla nahoda, ze jo?",
            "Mizerna rana, ale urazila.",
            "Krev? To je dekorace.",
            "No do prdele, trefa.",
            "Jestli to zopakujes, budu protivny.",
            "Bolest je jen nazor tela.",
            "Ty fakt zkousis prezit?",
            "Au. Moje ego ma diru.",
            "Konecne neco zajimaveho.",
            "Tak tohle byla drzost.",
            "Jsi otravny jak kletba.",
            "Hnusne, ale ucinne.",
            "To bolelo vic nez pravda.",
            "Pekna rana, ty potvoro.",
            "Prestavam se bavit.",
            "Moje zebro nesouhlasi.",
            "Tohle si zapamatuju."
        };

        private static readonly string[] HitPlayer =
        {
            "Sedlo? Sedlo.",
            "A hrdina zakopava o realitu.",
            "Tohle bylo za vsechny vazy.",
            "Krasne krupnuti.",
            "Mas tam novy otvor.",
            "Dalsi lekce zdarma.",
            "Tak se bojuje, zoufalce.",
            "Vidis? Ja taky umim kliknout.",
            "Au pro tebe, radost pro me.",
            "Tvoje zebra zpivaji.",
            "Ten zvuk se neomrzi.",
            "Pekny bar zdraví, skoda ho.",
            "Uhnout bylo v menu.",
            "Trefil jsem te do optimismu.",
            "Arogance minus osm.",
            "Maso reaguje predvidatelne.",
            "Tohle si dej do buildu.",
            "Pekne jsi to chytil oblicejem.",
            "Dalsi duvod k restartu.",
            "Neboj, bude hur.",
            "Jsi mekci nez moje vymluvy.",
            "To byl podpis, ne utok.",
            "Smrt ti posila pozdrav.",
            "Drzis se? Skoda.",
            "Zasah do hrdinske image."
        };

        private static readonly string[] Dying =
        {
            "Typicky. Zabity amatorem.",
            "Rekl bych posledni slova, ale kaslu na to.",
            "Do pekla... zase.",
            "Tohle neni konec, jen ostuda.",
            "Moje smrt ma lepsi timing nez ty.",
            "Zapis si to, mels kliku.",
            "Umiram, ale porad soudim.",
            "Tak jo, tohle bylo hnusne.",
            "Pillar mi to strhne z vyplaty.",
            "Konecne pauza od tehle prace.",
            "Nesnasim hrdiny.",
            "Moje posledni myslenka: blbec.",
            "Au revoir, ty tragedie.",
            "Tohle bude v reportu.",
            "Jdu si stezovat do temnoty.",
            "Fajn. Jedna nula pro maso.",
            "Nechavam ti svoje zklamani.",
            "Umrit rukou turisty, fakt super.",
            "Sakra. Mel jsem zustat v portalu.",
            "Tohle neni kanon.",
            "Nekdo vypnete ten hnusny svet.",
            "Jestli dropnu loot, tak naschval spatny.",
            "Konec smeny.",
            "Tma si najme kohokoliv.",
            "No tak to me poser.",
            "Vyhrals. Nebud na to pysny.",
            "Moje mrtvola ma vic stylu.",
            "Dneska se mi fakt nedari.",
            "Rikam to naposled: au.",
            "Tohle jsem si nezaslouzil. Asi."
        };

        public static string Get(EnemyBarkEvent barkEvent)
        {
            var lines = barkEvent switch
            {
                EnemyBarkEvent.Spawn => Spawn,
                EnemyBarkEvent.Searching => Searching,
                EnemyBarkEvent.Damaged => Damaged,
                EnemyBarkEvent.HitPlayer => HitPlayer,
                EnemyBarkEvent.Dying => Dying,
                _ => Searching
            };

            return lines[Random.Range(0, lines.Length)];
        }
    }
}
