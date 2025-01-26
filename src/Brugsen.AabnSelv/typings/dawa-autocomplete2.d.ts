declare module "dawa-autocomplete2" {
    interface DawaAutocomplete {
        destroy(): void;
    }
    interface Options {
        select: (selected: Selected) => void
    }
    interface Selected {
        type: "vejnavn" | "adresse" | "adgangsadresse";
        tekst: string;
        caretpos: number;
        forslagstekst: string;
        data: Record<string, any>;
    }    
    function dawaAutocomplete(element: HTMLInputElement, options: Options): DawaAutocomplete;
}