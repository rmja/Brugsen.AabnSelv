import { DefaultOptions, IntlTelInputCustomAttribute } from "./attribute";
import { IContainer, Registration } from "aurelia";
import { Options } from "./options";

export class IntlTelInputConfiguration {
  public static register(container: IContainer) {
    container.register(IntlTelInputCustomAttribute);
  }

  public static customize(defaultOptions: Partial<Options>) {
    if (!defaultOptions.loadUtils) {
      defaultOptions.loadUtils = () => import("intl-tel-input/utils");
    }

    return {
      register: (container: IContainer) => {
        this.register(container);

        container.register(
          Registration.instance(DefaultOptions, defaultOptions),
        );
      },
    };
  }
}
