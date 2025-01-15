export const red: LaesoeCard = {
  id: "red",
  name: "Rødt",
  colorClass: "danger",
};

export const blue: LaesoeCard = {
  id: "blue",
  name: "Blåt",
  colorClass: "primary",
};

export const green: LaesoeCard = {
  id: "green",
  name: "Grønt",
  colorClass: "success",
};

interface LaesoeCard {
  id: "red" | "blue" | "green";
  name: string;
  colorClass: string;
}
