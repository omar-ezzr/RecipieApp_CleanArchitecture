import { ComponentFixture, TestBed } from '@angular/core/testing';
import { activatedRouteStub } from '../../testing/route.stub';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ProfileComponent } from './profile.component';
import { AuthService } from '../../services/auth.service';
import { FollowService } from '../../services/follow.service';
import { UserProfileService } from '../../services/user-profile.service';

describe('ProfileComponent', () => {
  let fixture: ComponentFixture<ProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        { provide: ActivatedRoute, useValue: activatedRouteStub },
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { ...activatedRouteStub, paramMap: of(convertToParamMap({ id: 'user-1' })) } },
        {
          provide: UserProfileService,
          useValue: {
            getProfile: () => of({ id: 'user-1', displayName: 'Cook', followerCount: 0, followingCount: 0, recipeCount: 0 }),
            getRecipes: () => of({ items: [], total: 0, page: 1, pageSize: 10, totalPages: 0 })
          }
        },
        { provide: FollowService, useValue: { status: () => of({ isFollowing: false }), followers: () => of({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 }) } },
        { provide: AuthService, useValue: { isLoggedIn: () => true, getCurrentUserId: () => 'user-1', isAdmin: () => false } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileComponent);
  });

  it('renders public profile without email fields', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance.profile?.displayName).toBe('Cook');
  });
});
