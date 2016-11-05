# Krkal 3

Krkal 3 je nedokončený herní engine.

Vznikl jako diplomka na MFF-UK v roce 2007. Snažil jsem se odstranit neduhy z verze 2.4. Od základu jsem překopal skriptovací jazyk a napsal nový kompilátor. Krkal je nyní tvořen vyměnitelnými moduly, což znamená daleko obecnější použitelnost. V roce 2009 byly práce zastaveny. Hotovy jsou komponenty okolo jazyka a kompilátoru. Díky modularitě by se daly obstojně využít v jiném herním enginu. Bohužel tohle nestačí k oživení Krkala (či jiné hry) ve verzi 3.0.

## Nejdůležitější přínos systému Krkal:
Speciální objektově orientovaný jazyk, který dokáže snadno popsat simulaci rozmanitých, interagujících herních objektů. A který umožňuje snadnou rozšiřitelnost. Kdykoli máte hru s mnoha objekty, které nějakým způsobem žijí, vzájemně se ovlivňují, máte celou řadu obecných i speciálních pravidel a víte, že budou přicházet jak nové objekty, tak nová pravidla, Krkal je jednoznačná volba.

## Další featury
* **Krkal je objektově orientovaný.**  
* **Krkal je založen na skriptování.**  
* Objekty žijí a vzájemně se ovlivňují. Komunikaci a synchronizaci zajišťuje **systém zasílání zpráv**.  
* **Krkal je množinově orientovaný.** Vše, co jste pojmenovali, můžete seskupovat do množin. (objekty, metody, grafiku, abstraktní pojmy, ...) Množiny jednak ovlivňují chování celého systému, jednak s nimi můžete pracovat přímo ve skriptech, existuje speciální typ pro množiny. Příkladem využití množin je objektová dědičnost:  
* **Vícenásobná dědičnost.** Chcete vytvořit nový objekt, který bude možno 1) strkat 2) zničit 3) při zničení vybouchne 4) padá do děr 5) posouvají ho pásy 6) bojí se ho šneci... Prostě ho odděďte od předků s požadovanými vlastnostmi a je to!  
* **Rozšiřitelnost.** Skriptování a celý Krkal je navržen tak, aby bylo možné do her jednoduše zasahovat. Můžete si třeba vymyslet speciální úchylnou příšeru, vlastnost, změnu pravidel, ... a přidat ji jen do jednoho levelu. Krkal je navržen tak, aby to bylo jednoduché a nemuseli jste kvůli tomu přeruchat polovinu kódu. (To vše díky dědičnosti, množinám, safe metodám a verzování.)  
* **Verzování.** Krkal je od základu navržen tak, aby umožňoval práci na hře více lidem najednou. Krkal jednak zajišťuje, aby si lidé vzájemně nelezli do zelí, dále, aby se pak všechny ty modifikace daly dát dohromady a aby to fungovalo.  
* **Podpora jazykových lokalizací.**  
* **Jednoduchost**  
* **Bezpečnost**  
* **Síla** - zahrajte si Krkala a pochopíte, co engine dokáže a to jde zatím o tu úplně první verzi.    
* **Krkal není knihovna funkcí**, ale runtime, který se o herní objekty stará.  
* **Modularita** - propojte si Krkala s vlastním enginem.  
* Kompilátor překládá Krkal C do C++ a to je pak dále přeloženo do dll modulu.
* Kompilátor dokáže spolupracovat s editorem, online poskytuje typovou informaci a informaci pro chytré napovídání při psaní (Intellisense).

## Komponenty Krkala 3.0: 
**Už hotové (nebo téměř):**  
Kompilátor, Generátor kódu, Integrované prostředí pro psaní skriptů (dnes by bylo lepší integrovat do MS Visual Studia), Krkal Runtime (Kernel), File system a aplikace Sample, která to všechno dává dohromady a slouží jako demo. 

**ToDo:**  
Nový grafický engine, Editor levelů, či komponenta pro herní GUI, Hud, ... 

