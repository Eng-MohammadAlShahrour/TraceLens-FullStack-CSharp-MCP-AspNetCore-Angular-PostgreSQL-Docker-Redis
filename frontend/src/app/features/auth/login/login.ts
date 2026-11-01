import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  private signalr = inject(SignalRService);

  username = 'admin';
  password = 'Admin123!';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (!this.username.trim() || !this.password) {
      this.error.set('Username and password are required.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: () => {
        this.loading.set(false);
        this.signalr.start();
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        // OpenIddict returns 400 (invalid_grant) for bad credentials; 401 also possible
        const isInvalidCredentials = err?.status === 400 || err?.status === 401;
        this.error.set(
          isInvalidCredentials
            ? 'Invalid username or password.'
            : 'Login failed. Is the API running?'
        );
      }
    });
  }
}
