import "dawa-autocomplete2/css/dawa-autocomplete2.css";

import { DawaAutocomplete, dawaAutocomplete } from "dawa-autocomplete2";
import {
  ICustomAttributeViewModel,
  INode,
  customAttribute,
  resolve,
} from "aurelia";

@customAttribute("dawa-autocomplete")
export class DawaAutocompleteCustomAttribute
  implements ICustomAttributeViewModel
{
  private element = resolve(INode) as HTMLInputElement;
  private instance?: DawaAutocomplete;

  binding() {
    this.instance = dawaAutocomplete(this.element, {
      select: (selected) => {
        this.element.value = selected.tekst;
      },
    });
  }

  unbinding() {
    this.instance?.destroy();
    this.instance = undefined;
  }
}
