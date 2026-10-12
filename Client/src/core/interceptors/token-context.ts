import { HttpContext, HttpContextToken } from "@angular/common/http";
import { BYPASS_INTERCEPTOR } from "./error.interceptor";

export enum TokenType {
  User = 'user',
  Customer = 'customer',
  None = 'none'
}


export const TOKEN_TYPE = new HttpContextToken<TokenType>(() => TokenType.User);

// ⭐ Servislerde tek satırda kullanabilmen için helper
export function withAuthOptions(type: TokenType, bypass: boolean = false) {
  const context = new HttpContext()
    .set(TOKEN_TYPE, type);

  if (bypass) {
    context.set(BYPASS_INTERCEPTOR, true);
  }

  return { context };
}

/**
 * ⭐ HttpClient options objesi döner — direkt 3. parametre olarak kullanılır.
 *
 * Kullanım:
 *   this.http.get(url, withAuthOptions(TokenType.Customer));
 *   this.http.post(url, body, withAuthOptions(TokenType.None));
 */