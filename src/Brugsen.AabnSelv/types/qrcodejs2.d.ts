declare module "qrcodejs2" {
  export default class QRCode {
    constructor(element: HTMLElement, text: string);
    constructor(element: HTMLElement, options: Options);
    makeCode(text: string): void;
    clear(): void;
  }

  export interface Options {
    text: string;
    width?: number;
    height?: number;
    colorDark?: string;
    colorLight?: string;
  }
}
