import { Component, OnInit } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { environment } from '@env/environment';
import { OAuthService } from 'angular-oauth2-oidc';
import { HomeService } from './home.service';
interface Tenant {
  id: number;
  name: string;
  url: string;
}
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  tenants: Tenant[] = [
    {
      id: 1,
      name: 'Tenant 1',
      url: '',
    },
    {
      id: 2,
      name: 'Tenant 2',
      url: '',
    },
  ];
  error = null;
  result = {};
  constructor(
    private authService: AuthService,
    private oauthService: OAuthService,
    private homeSer: HomeService
  ) {}

  ngOnInit(): void {
    console.log(this.parseJwt(this.authService.accessToken));
  }
  // toggle() {
  //   if (environment.IssuerUri == 'https://tenant1.tenants.local:5000') {
  //     environment.IssuerUri = 'https://localhost:5000';
  //   } else {
  //     environment.IssuerUri = 'https://tenant1.tenants.local:5000';
  //   }
  //   this.authService.accessToken;
  // }
  async getInfo() {
    try {
      this.error = null;
      this.authService.refresh();
      // this.oauthService.loadDiscoveryDocumentAndTryLogin();
      this.result = this.parseJwt(this.authService.accessToken);
      // const result = await this.homeSer.getInfo();
      // console.log(result);
    } catch (error) {
      this.error = error;
    }
  }
  async changeTenant(index: number) {
    index = +index;
    console.log(this.tenants[index]);
  }
  async getImages() {
    try {
      this.error = null;
      this.result = await this.homeSer.getImages();
    } catch (error) {
      this.error = error;
    }
  }
  parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(function (c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join('')
    );

    return JSON.parse(jsonPayload);
  }
}
