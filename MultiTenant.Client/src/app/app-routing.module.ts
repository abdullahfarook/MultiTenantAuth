import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuardAuthenticadeOnly } from '@core/auth/auth-guard-authenticated-only.service';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { PagesModule } from './pages/pages.module';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  {
    path: 'home',
    canActivate: [AuthGuardAuthenticadeOnly],
    component: HomeComponent,
  },
  // 404 Not found
  { path: '**', redirectTo: 'not-found' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes), PagesModule],
  exports: [RouterModule],
})
export class AppRoutingModule {}
