import "dawa-autocomplete2/css/dawa-autocomplete2.css";

import { DawaAutocomplete, dawaAutocomplete } from "dawa-autocomplete2";
import {
  ICustomAttributeViewModel,
  INode,
  bindable,
  customAttribute,
  resolve,
} from "aurelia";

@customAttribute("dawa-autocomplete")
export class DawaAutocompleteCustomAttribute
  implements ICustomAttributeViewModel
{
  @bindable({ mode: "twoWay" }) id: string | null = null;
  private element = resolve(INode) as HTMLInputElement;
  private instance?: DawaAutocomplete;

  binding() {
    this.instance = dawaAutocomplete(this.element, {
      select: (selected) => {
        if (selected.type === "adresse") {
          this.element.value = selected.tekst;
          this.id = selected.data.id ?? null;
        }
      },
    });
  }

  unbinding() {
    this.instance?.destroy();
    this.instance = undefined;
  }
}
