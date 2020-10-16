# DigTheCard
Domyślna wartość rozdzielczości dla gry DigTheCard wynosi 1024x768.

Do utworzenia talii kart wykorzystano pliki graficzne z publicznie dostępnego produktu Vectorized Playing Cards 1.3, który został stworzony przez Chrisa Aguilara (https://sourceforge.net/projects/vector-cards/). 
Opracowany program odczytuje rozmieszczenie kart na stole oraz rozmieszczenie elementów graficznych na każdej karcie z zewnętrznych plików tekstowych, udostępnionych przez autora książki "Projektowanie gier przy użyciu środowiska Unity i języka C#" J. G. Bonda. Autor udostępnił również grafikę tła dla gry, oraz klasy Utils i PT_XMLReader jako narzędzia do opracowania projektu.

Założenia gry: celem gry jest pozbycie się wszystkich kart leżących na stole gracza. W danym momencie gry pozbyć można się tylko karty, której ranga jest o jeden większa lub mniejsza od rangi karty będącej celem (pojedyncza karta na środku ekranu). Gdy gracz nie ma możliwości zagrania żadnej z dostępnych kart, wybiera nowy cel ze stosu kart do pobrania (prawy górny róg ekranu). Każda kolejna zagrana w serii karta (to znaczy do momentu wybrania przez gracza nowego celu ze stosu kart do pobrania) jest nagradzana większą wartością punktową (za pozbycie się pierwszej karty w serii gracz otrzyma 1 pkt, za pozbycie się drugiej 2 pkt itd.)
W grze występują również złote karty, których zagranie skutkuje podwojeniem premii punktowej za zagranie danej karty w serii (jeśli gracz jako trzecią kartę w serii zagra kartę złotą, otrzyma 6 punktów zamiast 3). 
Gra kończy się w chwili, gdy gracz pozbył się wszystkich kart ze swojego stołu (wygrana), lub gdy skończą się wszystkie karty na stosie kart do pobrania, i gracz nie będzie miał już możliwości wykonania ruchu (przegrana).

Sterowanie: sterowanie w grze odbywa się wyłącznie przy użyciu myszki. Aby zagrać wybraną kartę, należy na nią kliknąć.

Skrypty:
- Card - klasa przechowuje informację o każdej karcie w grze, oraz zarządza warstwami obiektów graficznych komponentu SpriteRenderer.
- CardDTC - jest to klasa dziedzicząca po klasie Card, przechowująca wszystkie informacje charakterystyczne dla gry DigTheCard.
- Deck - odczytuje dane z zewnętrznego pliku, i na ich podstawie tworzy talię kart.
- DigTheCard - klasa odpowiada za logikę gry oraz wywoływanie przemieszczających się wartości punktowych.
- FloatingScore - klasa odpowiadająca za przemieszczające się po ekranie wartości punktowe.
- Layout - klasa odczytująca z zewnętrznego pliku dane o położeniu każdej karty na ekranie, i rozmieszcza je w odpowiednich miejscach
- Scoreboard - klasa odpowiada za wyświetlanie wyniku gracza.
- ScoreManager - odpowiada za zarządzanie wynikiem gracza w zależności od zdarzenia, oraz pobierająca najwyższą uzyskaną wartość punktową ze słownika PlayerPrefs
- Utils, PT_XMLReader - klasy udostępnione przez autora ww. ksiązki jako narzędzia. W projekcie wykorzystano klasy do odczytania danych z zewnętrznych plików napisanych w języku xml, oraz do wykonania ruchu przemieszczającej wartości punktowej na podstawie krzywej Beziera.
