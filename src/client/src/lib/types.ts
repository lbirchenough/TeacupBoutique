export interface ProductListDto {
    id: string;               // Guid -> string
    name: string;
    description: string;
    colour?: string | null;   // nullable in C#
    price: number;            // decimal -> number
    servings: number;
    featuredPhotoUrl?: string | null;
  }