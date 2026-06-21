import { Injectable } from '@angular/core';

export interface Toast {
  message: string;
  type: 'success' | 'danger' | 'info' | 'warning';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts: Toast[] = [];

  show(message: string, type: 'success' | 'danger' | 'info' | 'warning' = 'info') {
    const toast: Toast = { message, type };
    this.toasts.push(toast);
    setTimeout(() => this.remove(toast), 4000);
  }

  success(message: string) {
    this.show(message, 'success');
  }

  error(message: string) {
    this.show(message, 'danger');
  }

  warning(message: string) {
    this.show(message, 'warning');
  }

  info(message: string) {
    this.show(message, 'info');
  }

  remove(toast: Toast) {
    this.toasts = this.toasts.filter(t => t !== toast);
  }
}
