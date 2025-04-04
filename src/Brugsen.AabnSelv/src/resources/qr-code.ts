import "./qr-code.css";

import {
  ICustomElementViewModel,
  INode,
  bindable,
  customElement,
  inject,
} from "aurelia";

import QRCode from "qrcodejs2";

@inject(INode)
@customElement("qr-code")
export class QrCodeCustomElement implements ICustomElementViewModel {
  private instance?: QRCode;
  @bindable({ callback: "handleChange" })
  value?: string;

  @bindable({ callback: "handleChange" })
  size: number = 200;

  constructor(private element: HTMLElement) {}

  attached() {
    this.instance = new QRCode(this.element, {
      text: this.value ?? "",
      width: this.size,
      height: this.size,
    });
  }

  detaching() {
    this.instance?.clear();
  }

  handleChange() {
    this.instance?.makeCode(this.value ?? "");
  }
}
