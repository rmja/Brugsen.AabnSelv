import { IRouteableComponent } from "@aurelia/router";
import { customElement } from "aurelia";
import template from "./receipt.html";

@customElement({ name: "receipt-page", template })
export class ReceiptPage implements IRouteableComponent {}
