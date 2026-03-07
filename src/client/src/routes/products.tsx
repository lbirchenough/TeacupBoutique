import { createFileRoute } from "@tanstack/react-router";
import { inventoryApi } from "../lib/inventoryApi";
import { useQuery } from "@tanstack/react-query";
import type { ProductListDto } from "../lib/types";


export const Route = createFileRoute('/products')({
    component: ProductsPage,
  })
  
  function ProductsPage() {

    const { isPending, isError, data, error } = useQuery({
        queryKey: ['products'],
        queryFn: inventoryApi.getProducts,
      })
    return (
        <div>
            <h1>Products</h1>
            {isPending && <div>Loading...</div>}
            {isError && <div>Error: {error.message}</div>}
            {data && <div>{data.length} products found</div>}
            <ul>
                {data?.map((product: ProductListDto) => (
                    <li key={product.id}>{product.name}</li>
                ))}
            </ul>
        </div>
    )
  }