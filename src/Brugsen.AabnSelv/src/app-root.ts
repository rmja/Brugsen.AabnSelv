import "bootstrap/dist/css/bootstrap.css";

import { ICustomElementViewModel, customElement } from "aurelia";

import { routes } from "@aurelia/router";
import template from "./app-root.html";

@routes([
  { path: "", redirectTo: "signup" },
  {
    id: "signup",
    path: "signup",
    component: import("./signup-page"),
    title: "Brugeroprettelse",
  },
  {
    id: "receipt",
    path: "receipt",
    component: import("./receipt-page"),
    title: "Kvittering",
  }
])
@customElement({
  name: "app-root",
  template,
})
export class AppRootCustomElement implements ICustomElementViewModel {
  constructor() {}
}
