import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FollowService } from './follow.service';
import { LikeService } from './like.service';
import { CommentService } from './comment.service';
import { FeedService } from './feed.service';
import { NotificationService } from './notification.service';
import { UserProfileService } from './user-profile.service';

describe('Phase 3 social services', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        FollowService,
        LikeService,
        CommentService,
        FeedService,
        NotificationService,
        UserProfileService
      ]
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses user follow endpoints', () => {
    const service = TestBed.inject(FollowService);

    service.follow('user-2').subscribe();
    const follow = http.expectOne('http://localhost:5130/api/users/user-2/follow');
    expect(follow.request.method).toBe('POST');
    follow.flush(null);

    service.unfollow('user-2').subscribe();
    const unfollow = http.expectOne('http://localhost:5130/api/users/user-2/follow');
    expect(unfollow.request.method).toBe('DELETE');
    unfollow.flush(null);
  });

  it('uses recipe like endpoints', () => {
    const service = TestBed.inject(LikeService);

    service.like('recipe-1').subscribe();
    const like = http.expectOne('http://localhost:5130/api/recipes/recipe-1/likes');
    expect(like.request.method).toBe('POST');
    like.flush(null);

    service.getStatus('recipe-1').subscribe(result => {
      expect(result).toEqual({ isLiked: true, likeCount: 12 });
    });
    const status = http.expectOne('http://localhost:5130/api/recipes/recipe-1/likes/status');
    expect(status.request.method).toBe('GET');
    status.flush({ isLiked: true, likeCount: 12 });
  });

  it('uses recipe comment endpoints', () => {
    const service = TestBed.inject(CommentService);

    service.create('recipe-1', { content: 'Great' }).subscribe();
    const create = http.expectOne('http://localhost:5130/api/recipes/recipe-1/comments');
    expect(create.request.method).toBe('POST');
    expect(create.request.body.content).toBe('Great');
    create.flush({ id: 'comment-1', recipeId: 'recipe-1', content: 'Great', createdAt: '', author: { id: 'user-1', displayName: 'Cook' } });
  });

  it('uses feed and notification endpoints', () => {
    TestBed.inject(FeedService).getFeed(2, 5).subscribe();
    const feed = http.expectOne('http://localhost:5130/api/feed?page=2&pageSize=5');
    expect(feed.request.method).toBe('GET');
    feed.flush({ items: [], total: 0, page: 2, pageSize: 5, totalPages: 0 });

    TestBed.inject(NotificationService).unreadCount().subscribe();
    const unread = http.expectOne('http://localhost:5130/api/notifications/unread-count');
    expect(unread.request.method).toBe('GET');
    unread.flush({ count: 0 });
  });

  it('uses profile endpoints without exposing email models', () => {
    const service = TestBed.inject(UserProfileService);

    service.updateCurrentProfile({ displayName: 'Cook', bio: 'Bio' }).subscribe();
    const req = http.expectOne('http://localhost:5130/api/users/me/profile');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body.email).toBeUndefined();
    req.flush({ id: 'user-1', displayName: 'Cook', followerCount: 0, followingCount: 0, recipeCount: 0 });
  });
});
