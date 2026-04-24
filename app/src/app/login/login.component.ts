import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule], 
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})


export class LoginComponent {
email= '';
password='';
error='';

constructor(private auth: AuthService, private router:Router){}

success = false;

login() {
  this.auth.login({
    email: this.email,
    password: this.password
  }).subscribe({
    next: (res: any) => {
      this.auth.saveTokens(res.accessToken, res.refreshToken);
      this.success = true;

      setTimeout(() => this.router.navigate(['/recipes']), 1000);
    },
    error: () => {
      this.error = 'Invalid email or password';
    }
  });
}

}
