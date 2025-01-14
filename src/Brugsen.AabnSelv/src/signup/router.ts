import { IRouteableComponent, routes } from "@aurelia/router";

import { customElement } from "aurelia";
import template from "./router.html";

@routes([
  {
    id: "signup",
    path: "",
    component: import("./signup"),
    title: "Brugeroprettelse",
  },
  {
    id: "store-confirmation",
    path: "store-confirmation",
    component: import("./store-confirmation"),
    title: "Butiksbekræftelse",
  },
  {
    id: "receipt",
    path: "receipt",
    component: import("./receipt"),
    title: "Kvittering",
  },
])
@customElement({ name: "signup-router", template })
export class SignupRouter implements IRouteableComponent {}
