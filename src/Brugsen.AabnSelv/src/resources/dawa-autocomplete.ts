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

  constructor() {
    this.onChange = this.onChange.bind(this);
  }

  binding() {
    this.instance = dawaAutocomplete(this.element, {
      select: (selected) => {
        if (selected.type === "adresse") {
          this.element.value = selected.tekst;
          this.id = selected.data.id ?? null;
        }
      },
    });
    this.element.addEventListener("change", this.onChange);
  }

  unbinding() {
    this.element.removeEventListener("change", this.onChange);
    this.instance?.destroy();
    this.instance = undefined;
  }

  // Not triggered when assigned with this.element.value = "..."
  private onChange() {
    this.id = null;
  }
}
