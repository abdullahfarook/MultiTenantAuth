import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class HomeService {
  apiUrl = 'https://localhost:5001';
  headers = {
    headers: {
      Authorization: `Bearer ${this.authSer.accessToken}`,
      'Content-Type': 'application/json',
    },
  };
  constructor(private http: HttpClient, private authSer: AuthService) {}
  getImages(): Promise<any[]> {
    const headers = {
      headers: {
        Authorization: `Bearer ${this.authSer.accessToken}`,
        'Content-Type': 'application/json',
      },
    };
    return this.http
      .get<any[]>(`${this.apiUrl}/api/images`, headers)
      .toPromise();
  }
  getInfo(): Promise<any> {
    return this.http.get<any>(`${this.apiUrl}/info`, this.headers).toPromise();
  }
}
