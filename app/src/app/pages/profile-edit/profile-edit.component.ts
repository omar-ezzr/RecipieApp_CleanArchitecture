import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UpdatePublicUserProfile } from '../../models/user-profile.model';
import { UserProfileService } from '../../services/user-profile.service';

@Component({
  selector: 'app-profile-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile-edit.component.html',
  styleUrl: './profile-edit.component.css'
})
export class ProfileEditComponent implements OnInit {
  form: UpdatePublicUserProfile = {
    displayName: '',
    bio: '',
    avatarUrl: '',
    countryCode: ''
  };
  isSaving = false;

  constructor(
    private profileService: UserProfileService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.profileService.getCurrentProfile().subscribe({
      next: profile => this.form = {
        displayName: profile.displayName,
        bio: profile.bio ?? '',
        avatarUrl: profile.avatarUrl ?? '',
        countryCode: profile.countryCode ?? ''
      },
      error: () => this.toastr.error('Failed to load profile')
    });
  }

  save(): void {
    if (!this.form.displayName.trim()) {
      this.toastr.error('Display name is required');
      return;
    }

    this.isSaving = true;
    this.profileService.updateCurrentProfile(this.form).subscribe({
      next: profile => {
        this.toastr.success('Profile updated');
        this.router.navigate(['/users', profile.id]);
      },
      error: error => {
        this.toastr.error(error?.error?.message || 'Failed to update profile');
        this.isSaving = false;
      }
    });
  }
}
