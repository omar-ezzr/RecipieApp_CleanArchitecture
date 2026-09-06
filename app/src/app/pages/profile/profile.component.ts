
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastrService } from '@openng/ngx-toastr';
import { PagedResult } from '../../models/paged-result.model';
import { Recipe } from '../../models/recipe.model';
import { PublicUserProfile, UserSummary } from '../../models/user-profile.model';
import { AuthService } from '../../services/auth.service';
import { FollowService } from '../../services/follow.service';
import { UserProfileService } from '../../services/user-profile.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { RecipeCardComponent } from '../../shared/components/recipe-card/recipe-card.component';

@Component({
    selector: 'app-profile',
    imports: [RouterModule, EmptyStateComponent, RecipeCardComponent],
    templateUrl: './profile.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  profile?: PublicUserProfile;
  recipes: Recipe[] = [];
  followers?: PagedResult<UserSummary>;
  following?: PagedResult<UserSummary>;
  isFollowing = false;
  isLoading = true;
  isFollowBusy = false;
  activeList: 'followers' | 'following' | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private profileService: UserProfileService,
    private followService: FollowService,
    public auth: AuthService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (!id) {
        this.router.navigate(['/recipes']);
        return;
      }

      this.loadProfile(id);
    });
  }

  get isOwnProfile(): boolean {
    return !!this.profile && this.auth.getCurrentUserId() === this.profile.id;
  }

  loadProfile(id: string): void {
    this.isLoading = true;
    this.profileService.getProfile(id).subscribe({
      next: profile => {
        this.profile = profile;
        this.isLoading = false;
        this.loadRecipes(profile.id);
        if (this.auth.isLoggedIn() && !this.isOwnProfile) {
          this.loadFollowStatus(profile.id);
        }
      },
      error: () => {
        this.toastr.error('Profile was not found');
        this.isLoading = false;
      }
    });
  }

  loadRecipes(id: string): void {
    this.profileService.getRecipes(id).subscribe({
      next: result => this.recipes = result.items,
      error: () => this.toastr.error('Failed to load profile recipes')
    });
  }

  loadFollowStatus(id: string): void {
    this.followService.status(id).subscribe({
      next: status => this.isFollowing = status.isFollowing,
      error: () => this.isFollowing = false
    });
  }

  toggleFollow(): void {
    if (!this.profile || this.isFollowBusy) {
      return;
    }

    this.isFollowBusy = true;
    const request = this.isFollowing ? this.followService.unfollow(this.profile.id) : this.followService.follow(this.profile.id);
    request.subscribe({
      next: () => {
        this.isFollowing = !this.isFollowing;
        this.profile!.followerCount += this.isFollowing ? 1 : -1;
        this.isFollowBusy = false;
      },
      error: () => {
        this.toastr.error('Failed to update follow');
        this.isFollowBusy = false;
      }
    });
  }

  showList(type: 'followers' | 'following'): void {
    if (!this.profile) {
      return;
    }

    this.activeList = type;
    const request = type === 'followers'
      ? this.followService.followers(this.profile.id)
      : this.followService.following(this.profile.id);

    request.subscribe({
      next: result => {
        if (type === 'followers') {
          this.followers = result;
        } else {
          this.following = result;
        }
      },
      error: () => this.toastr.error('Failed to load users')
    });
  }

  closeList(): void {
    this.activeList = null;
  }

  activeUsers(): UserSummary[] {
    return this.activeList === 'followers'
      ? this.followers?.items ?? []
      : this.following?.items ?? [];
  }

  trackRecipe(_index: number, recipe: Recipe): string {
    return recipe.id;
  }

  trackUser(_index: number, user: UserSummary): string {
    return user.id;
  }
}
