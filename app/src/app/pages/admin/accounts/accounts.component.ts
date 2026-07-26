import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';
import { UserManagementService } from '../../../services/user-management.service';
import { CreateUserAccountRequest, UserAccount, UserRole } from '../../../models/user-account.model';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.css'
})
export class AccountsComponent implements OnInit {
  accounts: UserAccount[] = [];
  roles: UserRole[] = ['User', 'Operator', 'Admin'];
  isLoading = false;
  errorMessage = '';
  search = '';
  selectedRole: UserRole | '' = '';
  selectedStatus: '' | 'active' | 'inactive' = '';
  page = 1;
  pageSize = 20;
  total = 0;
  showCreateForm = false;
  createRequest: CreateUserAccountRequest = this.emptyCreateRequest();

  constructor(
    private userManagement: UserManagementService,
    public auth: AuthService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userManagement.getPaged({
      page: this.page,
      pageSize: this.pageSize,
      search: this.search || undefined,
      role: this.selectedRole || undefined,
      isActive: this.selectedStatus === '' ? null : this.selectedStatus === 'active'
    }).subscribe({
      next: result => {
        this.accounts = result.items;
        this.total = result.total;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.isLoading = false;
      },
      error: error => {
        this.errorMessage = this.getErrorMessage(error, 'Failed to load accounts.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadAccounts();
  }

  createAccount(): void {
    this.userManagement.create(this.createRequest).subscribe({
      next: () => {
        this.toastr.success('Account created.');
        this.createRequest = this.emptyCreateRequest();
        this.showCreateForm = false;
        this.loadAccounts();
      },
      error: error => {
        this.toastr.error(this.getErrorMessage(error, 'Failed to create account.'));
      }
    });
  }

  changeRole(account: UserAccount, role: UserRole): void {
    this.userManagement.updateRole(account.id, { role }).subscribe({
      next: updated => {
        account.role = updated.role;
        account.isActive = updated.isActive;
        this.toastr.success('Role updated.');
      },
      error: error => this.toastr.error(this.getErrorMessage(error, 'Failed to update role.'))
    });
  }

  toggleStatus(account: UserAccount): void {
    const nextStatus = !account.isActive;

    this.userManagement.updateStatus(account.id, { isActive: nextStatus }).subscribe({
      next: updated => {
        account.isActive = updated.isActive;
        this.toastr.success(updated.isActive ? 'Account activated.' : 'Account deactivated.');
      },
      error: error => this.toastr.error(this.getErrorMessage(error, 'Failed to update status.'))
    });
  }

  deleteAccount(account: UserAccount): void {
    if (!confirm(`Delete account ${account.email}? This cannot be undone.`)) {
      return;
    }

    this.userManagement.delete(account.id).subscribe({
      next: () => {
        this.toastr.success('Account deleted.');
        this.loadAccounts();
      },
      error: error => this.toastr.error(this.getErrorMessage(error, 'Failed to delete account.'))
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }

    this.page = page;
    this.loadAccounts();
  }

  isCurrentUser(account: UserAccount): boolean {
    return this.auth.getCurrentUserId() === account.id;
  }

  trackAccount(_index: number, account: UserAccount): string {
    return account.id;
  }

  get totalPages(): number {
    return this.total === 0 ? 0 : Math.ceil(this.total / this.pageSize);
  }

  private emptyCreateRequest(): CreateUserAccountRequest {
    return {
      email: '',
      password: '',
      role: 'User'
    };
  }

  private getErrorMessage(error: any, fallback: string): string {
    const validationErrors = error?.error?.errors;

    if (validationErrors) {
      const firstError = Object.values(validationErrors)
        .flat()
        .find((message): message is string => typeof message === 'string');

      if (firstError) {
        return firstError;
      }
    }

    return error?.error?.message || error?.error?.title || error?.error || fallback;
  }
}
