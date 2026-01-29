import "./qr-code.css";

import { ICustomElementViewModel, bindable, customElement } from "aurelia";

import QRCode from "qrcode";

@customElement("qr-code")
export class QrCodeCustomElement implements ICustomElementViewModel {
  private canvas!: HTMLCanvasElement;

  @bindable({ callback: "handleChange" })
  value?: string;

  @bindable({ callback: "handleChange" })
  size: number = 200;

  attached() {
    this.render();
  }

  handleChange() {
    this.render();
  }

  private async render() {
    if (!this.canvas || !this.value) return;

    try {
      await QRCode.toCanvas(this.canvas, this.value, {
        width: this.size,
      });
    } catch (error) {
      console.error("Failed to render QR code:", error);
    }
  }
}
