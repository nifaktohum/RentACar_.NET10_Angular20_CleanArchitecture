import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// Selam! Ben senin API isteklerinin anayasasıyım. 
// Bir isteğin sahip olabileceği tüm özellikleri (nereye gidecek, başlığı ne, tipi ne) burada kurala bağlıyorum.
export interface RequestParameters {
  controller?: string;                       // Backend'deki hangi controller'a (odaya) vuracağımı söyler.
  action?: string;                           // O controller içindeki hangi fonksiyona (aksiyona) gideceğimi netleştirir.
  queryString?: any;                         // URL'in sonuna eklenecek filtreleri (?page=1&size=10 gibi) paket halinde taşır.
  headers?: HttpHeaders;                     // Güvenlik tokenı veya özel geçiş kartları gibi başlık bilgilerini tutarım.
  baseUrl?: string;                          // Eğer ana API adresimiz dışında bir yere gideceksek, o özel adresi saklarım.
  fullEndpoint?: string;                     // "Bana controller, action falan deme, direkt şu URL'e git" dediğin an bu devreye girer.
  responseType?: 'json' | 'arraybuffer' | 'blob' | 'text'; // Backend'den bana ne dönecek? Veri mi, dosya mı, düz metin mi? Onu bilmemi sağlar.
}

@Injectable({ providedIn: 'root' })


export class HttpClientService {
  private httpClient = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  // Ben gizli bir terziyim; bana verdiğin parametrelere göre backend'in anlayacağı tam URL yolunu dikerim.
  private createUrl(params: Partial<RequestParameters>): string {
    // Eğer bana özel bir baseUrl vermediysen ana adresi alır, yanına controller'ı ve varsa action'ı yapıştırıp tertemiz bir yol çıkarırım.
    const base = params.baseUrl || this.baseUrl;
    const controller = params.controller || '';
    const action = params.action ? `/${params.action}` : '';
    return `${base}/${controller}${action}`;
  }

  // Ben de bir diğer gizli işçiyim; bana verdiğin nesneleri (?sayfa=5&limit=10 gibi) URL'in arkasına takılacak vagonlara dönüştürürüm.
  private buildQuery(params: any): string {
    if (!params) return ''; // Eğer hiçbir şey göndermediysen yolu hiç kirletmem.
    if (typeof params === 'string') return `?${params}`; // Zaten string gönderdiysen başına sadece soru işareti koyar geçerim.
    const query = new URLSearchParams(params).toString(); // Obje gönderdiysen onu otomatik olarak URL formatına çeviririm.
    return query ? `?${query}` : '';
  }


  get<T>(params: Partial<RequestParameters>, id?: string): Observable<T> {
    // Önce "Direkt bir adrese mi gideceğim yoksa URL mi oluşturacağım?" ona bakarım.
    let url = params.fullEndpoint || this.createUrl(params);
    // Eğer spesifik bir kaydın ID'sini verdiysen onu da yola eklerim (/products/5 gibi).
    if (id) url += `/${id}`;
    // Varsa query string filtrelerimi de arkasına takarım.
    url += this.buildQuery(params.queryString);

    // Ve HttpClient abime derim ki: "Al bu adresi, varsa başlıkları ve veri tipini de götür, bana veriyi çek getir!"
    return this.httpClient.get<T>(url, { headers: params.headers, responseType: params.responseType as any });
  }

  // Selam, ben POST metoduyum. Genelde yeni bir veri ekleyeceğin zaman beni görevlendirirsin.
  post<T>(params: Partial<RequestParameters>, body: any): Observable<T> {
    // URL'imi hazırlar, sorgularımı eklerim.
    let url = params.fullEndpoint || this.createUrl(params);
    url += this.buildQuery(params.queryString);

    // GET'ten farkım: Yanıma senin gönderdiğin o 'body' (yani yeni eklenecek veri paketini) alır, backend'e fırlatırım.
    return this.httpClient.post<T>(url, body, { headers: params.headers, responseType: params.responseType as any });
  }

  // Ben de PUT metoduyum. Mevcut bir veriyi tamamen güncellemek ya da değiştirmek istediğinde sahneye çıkarım.
  put<T>(params: Partial<RequestParameters>, body: any): Observable<T> {
    // Tıpkı POST gibi çalışırım, URL'i kurar, query'leri eklerim.
    let url = params.fullEndpoint || this.createUrl(params);
    url += this.buildQuery(params.queryString);

    // Güncellenecek yeni paketi (body) sırtlanır, backend'deki ilgili adrese teslim ederim.
    return this.httpClient.put<T>(url, body, { headers: params.headers, responseType: params.responseType as any });
  }

  // Ve son olarak ben DELETE! Veri silineceği zaman baltayı elime alır, yıkıma giderim.
  delete<T>(params: Partial<RequestParameters>, id?: string): Observable<T> {
    // Hangi adrese gideceğimi belirlerim.
    let url = params.fullEndpoint || this.createUrl(params);
    // Genelde "Şu ID'li veriyi sil" dersin, o yüzden ID varsa yola eklerim (/products/5 gibi).
    if (id) url += `/${id}`;
    url += this.buildQuery(params.queryString);

    // HttpClient abime hedefi gösteririm: "Bu adresteki veriyi yok et!"
    return this.httpClient.delete<T>(url, { headers: params.headers, responseType: params.responseType as any });
  }
}

/* KUllanım örnekleri.  ===>

  export class CarService {
      // // Senin modern enjeksiyon yönteminle HTTP motorumuzu çağırıyoruz
      private http = inject(HttpClientService);

      // // 1. Senaryo: Tüm arabaları listeleme (GET) -> /api/cars
      getCars(): Observable<Car[]> {
        return this.http.get<Car[]>({
          controller: 'cars'
        });
      }

      // // 2. Senaryo: ID ile tek bir araba getirme (GET) -> /api/cars/5
      getCarById(id: string): Observable<Car> {
        return this.http.get<Car>({
          controller: 'cars'
        }, id);
      }

      // // 3. Senaryo: Filtreleme ve Sayfalama ile araba getirme (QueryString ile GET) -> /api/cars?page=1&size=10&brand=BMW
      getCarsFiltered(page: number, size: number, brand?: string): Observable<Car[]> {
        return this.http.get<Car[]>({
          controller: 'cars',
          queryString: { page, size, brand } // // Obje olarak gönderdik, servis otomatik stringe çevirecek!
        });
      }

      // // 4. Senaryo: Yeni araba ekleme (POST) -> /api/cars
      createCar(car: Partial<Car>): Observable<Car> {
        return this.http.post<Car>({
          controller: 'cars'
        }, car);
      }

      // // 5. Senaryo: Özel bir action'a POST isteği atma -> /api/cars/apply-discount
      applyDiscountToCar(carId: string, rate: number): Observable<any> {
        return this.http.post<any>({
          controller: 'cars',
          action: 'apply-discount',
          queryString: { carId, rate }
        }, {}); // // Body boş olsa bile gönderiyoruz
      }

      // // 6. Senaryo: Araba güncelleme (PUT) -> /api/cars
      updateCar(car: Car): Observable<Car> {
        return this.http.put<Car>({
          controller: 'cars'
        }, car);
      }

      // // 7. Senaryo: Araba silme (DELETE) -> /api/cars/5
      deleteCar(id: string): Observable<any> {
        return this.http.delete<any>({
          controller: 'cars'
        }, id);
      }
}

*/