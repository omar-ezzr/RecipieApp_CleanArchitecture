import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  email = '';
  displayName = '';
  password = '';
  message = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  register() {
    this.auth.register({
      displayName: this.displayName,
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.message = 'Account created and waiting for administrator approval.';
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
error: (err) => {
  this.error =
    err.error?.message ||
    err.error?.error ||
    'Registration failed';

}
    });
  }
}
