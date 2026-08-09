export interface PublicUserProfile {
  id: string;
  displayName: string;
  bio?: string | null;
  avatarUrl?: string | null;
  countryCode?: string | null;
  followerCount: number;
  followingCount: number;
  recipeCount: number;
}

export interface UpdatePublicUserProfile {
  displayName: string;
  bio?: string | null;
  avatarUrl?: string | null;
  countryCode?: string | null;
}

export interface UserSummary {
  id: string;
  displayName: string;
  avatarUrl?: string | null;
  countryCode?: string | null;
  isFollowing: boolean;
}
