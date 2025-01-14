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
import { Iti, SomeOptions } from "intl-tel-input";

import intlTelInput from "intl-tel-input";

export const DefaultOptions = Symbol("DefaultOptions123");

@inject(optional(DefaultOptions))
@customAttribute("intl-tel-input")
export class IntlTelInputCustomAttribute implements ICustomAttributeViewModel {
  private element = resolve(INode) as HTMLInputElement;
  private instance!: Iti;

  @bindable({ primary: true, mode: BindingMode.twoWay })
  value?: string;

  @bindable()
  options?: intlTelInput.Options;

  constructor(private defaultOptions: SomeOptions) {
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
  }

  unbinding() {
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
