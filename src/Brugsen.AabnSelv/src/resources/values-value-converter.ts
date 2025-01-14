import { valueConverter } from "aurelia";

@valueConverter("values")
export class ValuesValueConverter {
  toView(obj: Record<string, unknown>) {
    return Object.values(obj);
  }
}
