import { IRouteViewModel } from "@aurelia/router";
import { customElement } from "aurelia";
import template from "./receipt-step.html";

@customElement({ name: "receipt-step", template })
export class ReceiptStep implements IRouteViewModel {}
