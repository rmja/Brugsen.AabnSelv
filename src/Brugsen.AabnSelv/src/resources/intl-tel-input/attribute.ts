import "intl-tel-input/styles";
import "./styles.css";

import {
  BindingMode,
  ICustomAttributeViewModel,
  INode,
  bindable,
  customAttribute,
  inject,
  optional,
  resolve,
} from "aurelia";
import { Iti } from "intl-tel-input";

import intlTelInput from "intl-tel-input";
import { Options } from "./options";

export const DefaultOptions = Symbol("DefaultOptions123");

@inject(optional(DefaultOptions))
@customAttribute({ name: "intl-tel-input", defaultProperty: "value"})
export class IntlTelInputCustomAttribute implements ICustomAttributeViewModel {
  private element = resolve(INode) as HTMLInputElement;
  private instance!: Iti;
  private mutationObserver = new MutationObserver(() => {
    const parent = this.element.parentElement;
    if (!parent) {
      return;
    }
    const isInvalid = this.element.classList.contains("is-invalid");
    if (isInvalid) {
      parent.classList.add("is-invalid");
    } else {
      parent.classList.remove("is-invalid");
    }
  });

  @bindable({ mode: BindingMode.twoWay })
  value?: string;

  @bindable()
  options?: intlTelInput.Options;

  constructor(private defaultOptions: Partial<Options>) {
    this.handleChange = this.handleChange.bind(this);
  }

  binding() {
    const formFloatingEnabled =
      this.element.parentElement?.classList.contains("form-floating");
    const floatingLabel = this.element.nextElementSibling;

    const options = Object.assign({}, this.defaultOptions, this.options);
    if (formFloatingEnabled) {
      options.autoPlaceholder = "off";
    }
    this.instance = intlTelInput(this.element, options);
    if (this.element.classList.contains("form-control")) {
      this.element.parentElement!.classList.add("iti-with-form-control");
    }

    if (formFloatingEnabled && floatingLabel?.tagName === "LABEL") {
      this.element.parentElement!.classList.add("form-floating");
      this.element.parentElement!.appendChild(floatingLabel);
    }

    if (this.value) {
      this.instance.setNumber(this.value);
    }

    this.element.addEventListener("countrychange", this.handleChange);
    this.element.addEventListener("keyup", this.handleChange);

    this.mutationObserver.observe(this.element, {
      attributes: true,
      attributeFilter: ["class"],
    });
  }

  unbinding() {
    this.mutationObserver.disconnect();
    this.element.removeEventListener("countrychange", this.handleChange);
    this.element.removeEventListener("keyup", this.handleChange);
    this.instance.destroy();
  }

  valueChanged() {
    this.instance.setNumber(this.value!);
  }

  private handleChange() {
    this.value = this.instance.getNumber();
  }
}
