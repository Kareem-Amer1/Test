import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { useUpdateProfile } from "../useProfile";
import type { UserProfile } from "../profile.types";

const schema = z.object({
  fullName: z.string().min(2, "At least 2 characters"),
});

type FormData = z.infer<typeof schema>;

interface Props {
  profile: UserProfile;
}

export function ProfileNameForm({ profile }: Props) {
  const { t } = useTranslation();
  const updateProfile = useUpdateProfile();
  const { register, handleSubmit, reset, formState: { errors, isDirty } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { fullName: profile.fullName },
  });

  useEffect(() => {
    reset({ fullName: profile.fullName });
  }, [profile.fullName, reset]);

  const onSubmit = handleSubmit(async (values) => {
    try {
      await updateProfile.mutateAsync({ fullName: values.fullName.trim() });
      toast.success(t("profile.updated", "Profile updated."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="space-y-1.5">
        <Label htmlFor="profile-fullName">{t("profile.fullName", "Full name")}</Label>
        <Input id="profile-fullName" {...register("fullName")} />
        {errors.fullName && (
          <p className="text-xs text-destructive">{errors.fullName.message}</p>
        )}
      </div>
      <Button type="submit" disabled={!isDirty || updateProfile.isPending}>
        {t("profile.saveProfile", "Save profile")}
      </Button>
    </form>
  );
}
